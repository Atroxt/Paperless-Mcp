using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using Paperless.Mcp.Models.Processings.Tags;
using Paperless.Mcp.Models.Processings.Tags.Exceptions;
using Paperless.Mcp.Services.Foundations.Tags;
using Paperless.Mcp.Services.Processings.Tags;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Tags
{
    [TestClass]
    public class TagProcessingServiceTests
    {
        private readonly Mock<ITagService> tagServiceMock;
        private readonly ITagProcessingService tagProcessingService;

        public TagProcessingServiceTests()
        {
            this.tagServiceMock = new Mock<ITagService>();

            this.tagProcessingService =
                new TagProcessingService(tagService: this.tagServiceMock.Object);
        }

        private static Tag CreateTag(int documentCount) =>
            new() { Id = 7, Name = "Rechnungen", DocumentCount = documentCount };

        // Der wichtigste Test dieser Schicht: ohne Bestaetigung darf nichts verschwinden.
        [TestMethod]
        public async Task ShouldNotDeleteOnDryRunAsync()
        {
            // given
            Tag storedTag = CreateTag(documentCount: 143);

            this.tagServiceMock.Setup(service =>
                service.RetrieveTagByIdAsync(storedTag.Id))
                    .ReturnsAsync(storedTag);

            // when
            TagRemovalPlan actualPlan =
                await this.tagProcessingService.RemoveTagByIdAsync(storedTag.Id, confirm: false);

            // then
            actualPlan.Removed.Should().BeFalse();
            actualPlan.TagId.Should().Be(storedTag.Id);
            actualPlan.Name.Should().Be("Rechnungen");
            actualPlan.DocumentCount.Should().Be(143);
            actualPlan.Summary.Should().Contain("Dry run");
            actualPlan.Summary.Should().Contain("143");

            this.tagServiceMock.Verify(service =>
                service.RetrieveTagByIdAsync(storedTag.Id),
                    Times.Once);

            this.tagServiceMock.Verify(service =>
                service.RemoveTagByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDeleteWhenConfirmedAsync()
        {
            // given
            Tag storedTag = CreateTag(documentCount: 2);

            this.tagServiceMock.Setup(service =>
                service.RetrieveTagByIdAsync(storedTag.Id))
                    .ReturnsAsync(storedTag);

            // when
            TagRemovalPlan actualPlan =
                await this.tagProcessingService.RemoveTagByIdAsync(storedTag.Id, confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();
            actualPlan.DocumentCount.Should().Be(2);
            actualPlan.Summary.Should().NotContain("Dry run");

            // Erst lesen, dann loeschen: der Bericht soll die Tragweite auch im
            // bestaetigten Fall nennen koennen.
            this.tagServiceMock.Verify(service =>
                service.RetrieveTagByIdAsync(storedTag.Id),
                    Times.Once);

            this.tagServiceMock.Verify(service =>
                service.RemoveTagByIdAsync(storedTag.Id),
                    Times.Once);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportWhenNoDocumentCarriesTheTagAsync()
        {
            // given
            Tag storedTag = CreateTag(documentCount: 0);

            this.tagServiceMock.Setup(service =>
                service.RetrieveTagByIdAsync(storedTag.Id))
                    .ReturnsAsync(storedTag);

            // when
            TagRemovalPlan actualPlan =
                await this.tagProcessingService.RemoveTagByIdAsync(storedTag.Id, confirm: false);

            // then
            actualPlan.Summary.Should().Contain("No document");

            this.tagServiceMock.Verify(service =>
                service.RetrieveTagByIdAsync(storedTag.Id),
                    Times.Once);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task ShouldThrowValidationExceptionIfIdIsInvalidAsync(int invalidTagId)
        {
            // when
            Func<Task<TagRemovalPlan>> removeTask = async () =>
                await this.tagProcessingService.RemoveTagByIdAsync(invalidTagId, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<TagProcessingValidationException>();

            // Eine ungueltige Id darf gar nicht erst beim Dienst ankommen
            this.tagServiceMock.Verify(service =>
                service.RetrieveTagByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.tagServiceMock.Verify(service =>
                service.RemoveTagByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapNotFoundToDependencyValidationAsync()
        {
            // given
            var tagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: new NotFoundTagException(
                        message: "Tag not found.",
                        innerException: new Exception()));

            this.tagServiceMock.Setup(service =>
                service.RetrieveTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(tagDependencyValidationException);

            // when
            Func<Task<TagRemovalPlan>> removeTask = async () =>
                await this.tagProcessingService.RemoveTagByIdAsync(99, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<TagProcessingDependencyValidationException>();

            this.tagServiceMock.Verify(service =>
                service.RetrieveTagByIdAsync(It.IsAny<int>()),
                    Times.Once);

            // Gescheitertes Lesen heisst: nicht loeschen
            this.tagServiceMock.Verify(service =>
                service.RemoveTagByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapUnexpectedErrorToProcessingServiceExceptionAsync()
        {
            // given
            this.tagServiceMock.Setup(service =>
                service.RetrieveTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

            // when
            Func<Task<TagRemovalPlan>> removeTask = async () =>
                await this.tagProcessingService.RemoveTagByIdAsync(7, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<TagProcessingServiceException>();

            this.tagServiceMock.Verify(service =>
                service.RetrieveTagByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.tagServiceMock.VerifyNoOtherCalls();
        }
    }
}
