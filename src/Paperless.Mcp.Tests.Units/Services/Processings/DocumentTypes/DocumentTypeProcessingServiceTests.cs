using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;
using Paperless.Mcp.Models.Processings.DocumentTypes;
using Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions;
using Paperless.Mcp.Services.Foundations.DocumentTypes;
using Paperless.Mcp.Services.Processings.DocumentTypes;

namespace Paperless.Mcp.Tests.Units.Services.Processings.DocumentTypes
{
    [TestClass]
    public class DocumentTypeProcessingServiceTests
    {
        private readonly Mock<IDocumentTypeService> documentTypeServiceMock;
        private readonly IDocumentTypeProcessingService documentTypeProcessingService;

        public DocumentTypeProcessingServiceTests()
        {
            this.documentTypeServiceMock = new Mock<IDocumentTypeService>();

            this.documentTypeProcessingService =
                new DocumentTypeProcessingService(documentTypeService: this.documentTypeServiceMock.Object);
        }

        private static DocumentType CreateDocumentType(int documentCount) =>
            new() { Id = 7, Name = "Rechnungen", DocumentCount = documentCount };

        // Der wichtigste Test dieser Schicht: ohne Bestaetigung darf nichts verschwinden.
        [TestMethod]
        public async Task ShouldNotDeleteOnDryRunAsync()
        {
            // given
            DocumentType storedDocumentType = CreateDocumentType(documentCount: 143);

            this.documentTypeServiceMock.Setup(service =>
                service.RetrieveDocumentTypeByIdAsync(storedDocumentType.Id))
                    .ReturnsAsync(storedDocumentType);

            // when
            DocumentTypeRemovalPlan actualPlan =
                await this.documentTypeProcessingService.RemoveDocumentTypeByIdAsync(storedDocumentType.Id, confirm: false);

            // then
            actualPlan.Removed.Should().BeFalse();
            actualPlan.DocumentTypeId.Should().Be(storedDocumentType.Id);
            actualPlan.Name.Should().Be("Rechnungen");
            actualPlan.DocumentCount.Should().Be(143);
            actualPlan.Summary.Should().Contain("Dry run");
            actualPlan.Summary.Should().Contain("143");

            this.documentTypeServiceMock.Verify(service =>
                service.RetrieveDocumentTypeByIdAsync(storedDocumentType.Id),
                    Times.Once);

            this.documentTypeServiceMock.Verify(service =>
                service.RemoveDocumentTypeByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentTypeServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDeleteWhenConfirmedAsync()
        {
            // given
            DocumentType storedDocumentType = CreateDocumentType(documentCount: 2);

            this.documentTypeServiceMock.Setup(service =>
                service.RetrieveDocumentTypeByIdAsync(storedDocumentType.Id))
                    .ReturnsAsync(storedDocumentType);

            // when
            DocumentTypeRemovalPlan actualPlan =
                await this.documentTypeProcessingService.RemoveDocumentTypeByIdAsync(storedDocumentType.Id, confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();
            actualPlan.DocumentCount.Should().Be(2);
            actualPlan.Summary.Should().NotContain("Dry run");

            // Erst lesen, dann loeschen: der Bericht soll die Tragweite auch im
            // bestaetigten Fall nennen koennen.
            this.documentTypeServiceMock.Verify(service =>
                service.RetrieveDocumentTypeByIdAsync(storedDocumentType.Id),
                    Times.Once);

            this.documentTypeServiceMock.Verify(service =>
                service.RemoveDocumentTypeByIdAsync(storedDocumentType.Id),
                    Times.Once);

            this.documentTypeServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportWhenNoDocumentCarriesTheDocumentTypeAsync()
        {
            // given
            DocumentType storedDocumentType = CreateDocumentType(documentCount: 0);

            this.documentTypeServiceMock.Setup(service =>
                service.RetrieveDocumentTypeByIdAsync(storedDocumentType.Id))
                    .ReturnsAsync(storedDocumentType);

            // when
            DocumentTypeRemovalPlan actualPlan =
                await this.documentTypeProcessingService.RemoveDocumentTypeByIdAsync(storedDocumentType.Id, confirm: false);

            // then
            actualPlan.Summary.Should().Contain("No document");

            this.documentTypeServiceMock.Verify(service =>
                service.RetrieveDocumentTypeByIdAsync(storedDocumentType.Id),
                    Times.Once);

            this.documentTypeServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task ShouldThrowValidationExceptionIfIdIsInvalidAsync(int invalidDocumentTypeId)
        {
            // when
            Func<Task<DocumentTypeRemovalPlan>> removeTask = async () =>
                await this.documentTypeProcessingService.RemoveDocumentTypeByIdAsync(invalidDocumentTypeId, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentTypeProcessingValidationException>();

            // Eine ungueltige Id darf gar nicht erst beim Dienst ankommen
            this.documentTypeServiceMock.Verify(service =>
                service.RetrieveDocumentTypeByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentTypeServiceMock.Verify(service =>
                service.RemoveDocumentTypeByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentTypeServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapNotFoundToDependencyValidationAsync()
        {
            // given
            var documentTypeDependencyValidationException =
                new DocumentTypeDependencyValidationException(
                    message: "DocumentType dependency validation error occurred, fix the errors and try again.",
                    innerException: new NotFoundDocumentTypeException(
                        message: "DocumentType not found.",
                        innerException: new Exception()));

            this.documentTypeServiceMock.Setup(service =>
                service.RetrieveDocumentTypeByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(documentTypeDependencyValidationException);

            // when
            Func<Task<DocumentTypeRemovalPlan>> removeTask = async () =>
                await this.documentTypeProcessingService.RemoveDocumentTypeByIdAsync(99, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentTypeProcessingDependencyValidationException>();

            this.documentTypeServiceMock.Verify(service =>
                service.RetrieveDocumentTypeByIdAsync(It.IsAny<int>()),
                    Times.Once);

            // Gescheitertes Lesen heisst: nicht loeschen
            this.documentTypeServiceMock.Verify(service =>
                service.RemoveDocumentTypeByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentTypeServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapUnexpectedErrorToProcessingServiceExceptionAsync()
        {
            // given
            this.documentTypeServiceMock.Setup(service =>
                service.RetrieveDocumentTypeByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

            // when
            Func<Task<DocumentTypeRemovalPlan>> removeTask = async () =>
                await this.documentTypeProcessingService.RemoveDocumentTypeByIdAsync(7, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentTypeProcessingServiceException>();

            this.documentTypeServiceMock.Verify(service =>
                service.RetrieveDocumentTypeByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.documentTypeServiceMock.VerifyNoOtherCalls();
        }
    }
}
