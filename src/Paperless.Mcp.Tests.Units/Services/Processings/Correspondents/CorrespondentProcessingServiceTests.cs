using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;
using Paperless.Mcp.Models.Processings.Correspondents;
using Paperless.Mcp.Models.Processings.Correspondents.Exceptions;
using Paperless.Mcp.Services.Foundations.Correspondents;
using Paperless.Mcp.Services.Processings.Correspondents;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Correspondents
{
    [TestClass]
    public class CorrespondentProcessingServiceTests
    {
        private readonly Mock<ICorrespondentService> correspondentServiceMock;
        private readonly ICorrespondentProcessingService correspondentProcessingService;

        public CorrespondentProcessingServiceTests()
        {
            this.correspondentServiceMock = new Mock<ICorrespondentService>();

            this.correspondentProcessingService =
                new CorrespondentProcessingService(correspondentService: this.correspondentServiceMock.Object);
        }

        private static Correspondent CreateCorrespondent(int documentCount) =>
            new() { Id = 7, Name = "Rechnungen", DocumentCount = documentCount };

        // Der wichtigste Test dieser Schicht: ohne Bestaetigung darf nichts verschwinden.
        [TestMethod]
        public async Task ShouldNotDeleteOnDryRunAsync()
        {
            // given
            Correspondent storedCorrespondent = CreateCorrespondent(documentCount: 143);

            this.correspondentServiceMock.Setup(service =>
                service.RetrieveCorrespondentByIdAsync(storedCorrespondent.Id))
                    .ReturnsAsync(storedCorrespondent);

            // when
            CorrespondentRemovalPlan actualPlan =
                await this.correspondentProcessingService.RemoveCorrespondentByIdAsync(storedCorrespondent.Id, confirm: false);

            // then
            actualPlan.Removed.Should().BeFalse();
            actualPlan.CorrespondentId.Should().Be(storedCorrespondent.Id);
            actualPlan.Name.Should().Be("Rechnungen");
            actualPlan.DocumentCount.Should().Be(143);
            actualPlan.Summary.Should().Contain("Dry run");
            actualPlan.Summary.Should().Contain("143");

            this.correspondentServiceMock.Verify(service =>
                service.RetrieveCorrespondentByIdAsync(storedCorrespondent.Id),
                    Times.Once);

            this.correspondentServiceMock.Verify(service =>
                service.RemoveCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.correspondentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDeleteWhenConfirmedAsync()
        {
            // given
            Correspondent storedCorrespondent = CreateCorrespondent(documentCount: 2);

            this.correspondentServiceMock.Setup(service =>
                service.RetrieveCorrespondentByIdAsync(storedCorrespondent.Id))
                    .ReturnsAsync(storedCorrespondent);

            // when
            CorrespondentRemovalPlan actualPlan =
                await this.correspondentProcessingService.RemoveCorrespondentByIdAsync(storedCorrespondent.Id, confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();
            actualPlan.DocumentCount.Should().Be(2);
            actualPlan.Summary.Should().NotContain("Dry run");

            // Erst lesen, dann loeschen: der Bericht soll die Tragweite auch im
            // bestaetigten Fall nennen koennen.
            this.correspondentServiceMock.Verify(service =>
                service.RetrieveCorrespondentByIdAsync(storedCorrespondent.Id),
                    Times.Once);

            this.correspondentServiceMock.Verify(service =>
                service.RemoveCorrespondentByIdAsync(storedCorrespondent.Id),
                    Times.Once);

            this.correspondentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportWhenNoDocumentCarriesTheCorrespondentAsync()
        {
            // given
            Correspondent storedCorrespondent = CreateCorrespondent(documentCount: 0);

            this.correspondentServiceMock.Setup(service =>
                service.RetrieveCorrespondentByIdAsync(storedCorrespondent.Id))
                    .ReturnsAsync(storedCorrespondent);

            // when
            CorrespondentRemovalPlan actualPlan =
                await this.correspondentProcessingService.RemoveCorrespondentByIdAsync(storedCorrespondent.Id, confirm: false);

            // then
            actualPlan.Summary.Should().Contain("No document");

            this.correspondentServiceMock.Verify(service =>
                service.RetrieveCorrespondentByIdAsync(storedCorrespondent.Id),
                    Times.Once);

            this.correspondentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task ShouldThrowValidationExceptionIfIdIsInvalidAsync(int invalidCorrespondentId)
        {
            // when
            Func<Task<CorrespondentRemovalPlan>> removeTask = async () =>
                await this.correspondentProcessingService.RemoveCorrespondentByIdAsync(invalidCorrespondentId, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<CorrespondentProcessingValidationException>();

            // Eine ungueltige Id darf gar nicht erst beim Dienst ankommen
            this.correspondentServiceMock.Verify(service =>
                service.RetrieveCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.correspondentServiceMock.Verify(service =>
                service.RemoveCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.correspondentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapNotFoundToDependencyValidationAsync()
        {
            // given
            var correspondentDependencyValidationException =
                new CorrespondentDependencyValidationException(
                    message: "Correspondent dependency validation error occurred, fix the errors and try again.",
                    innerException: new NotFoundCorrespondentException(
                        message: "Correspondent not found.",
                        innerException: new Exception()));

            this.correspondentServiceMock.Setup(service =>
                service.RetrieveCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(correspondentDependencyValidationException);

            // when
            Func<Task<CorrespondentRemovalPlan>> removeTask = async () =>
                await this.correspondentProcessingService.RemoveCorrespondentByIdAsync(99, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<CorrespondentProcessingDependencyValidationException>();

            this.correspondentServiceMock.Verify(service =>
                service.RetrieveCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            // Gescheitertes Lesen heisst: nicht loeschen
            this.correspondentServiceMock.Verify(service =>
                service.RemoveCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.correspondentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapUnexpectedErrorToProcessingServiceExceptionAsync()
        {
            // given
            this.correspondentServiceMock.Setup(service =>
                service.RetrieveCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

            // when
            Func<Task<CorrespondentRemovalPlan>> removeTask = async () =>
                await this.correspondentProcessingService.RemoveCorrespondentByIdAsync(7, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<CorrespondentProcessingServiceException>();

            this.correspondentServiceMock.Verify(service =>
                service.RetrieveCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.correspondentServiceMock.VerifyNoOtherCalls();
        }
    }
}
