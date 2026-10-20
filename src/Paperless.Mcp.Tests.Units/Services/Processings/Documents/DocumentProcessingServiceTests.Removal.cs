using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Documents
{
    public partial class DocumentProcessingServiceTests
    {
        private static Document CreateDocument() =>
            new() { Id = 13, Title = "Rechnung 4711" };

        [TestMethod]
        public async Task ShouldNotDeleteDocumentOnDryRunAsync()
        {
            // given
            Document storedDocument = CreateDocument();

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id))
                    .ReturnsAsync(storedDocument);

            // when
            DocumentRemovalPlan actualPlan =
                await this.documentProcessingService.RemoveDocumentByIdAsync(
                    storedDocument.Id, confirm: false);

            // then
            actualPlan.Removed.Should().BeFalse();
            actualPlan.Title.Should().Be("Rechnung 4711");
            actualPlan.Summary.Should().Contain("Dry run");
            actualPlan.Summary.Should().Contain("cannot be");

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RemoveDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDeleteDocumentWhenConfirmedAsync()
        {
            // given
            Document storedDocument = CreateDocument();

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id))
                    .ReturnsAsync(storedDocument);

            // when
            DocumentRemovalPlan actualPlan =
                await this.documentProcessingService.RemoveDocumentByIdAsync(
                    storedDocument.Id, confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RemoveDocumentByIdAsync(storedDocument.Id),
                    Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldNotReprocessOnDryRunAsync()
        {
            // given
            Document storedDocument = CreateDocument();

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id))
                    .ReturnsAsync(storedDocument);

            // when
            DocumentReprocessPlan actualPlan =
                await this.documentProcessingService.ReprocessDocumentByIdAsync(
                    storedDocument.Id, confirm: false);

            // then
            actualPlan.Started.Should().BeFalse();
            actualPlan.Summary.Should().Contain("Dry run");

            // Der Bericht muss auf den Verlust manueller Korrekturen hinweisen
            actualPlan.Summary.Should().Contain("manual corrections");

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.ReprocessDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReprocessWhenConfirmedAsync()
        {
            // given
            Document storedDocument = CreateDocument();

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id))
                    .ReturnsAsync(storedDocument);

            // when
            DocumentReprocessPlan actualPlan =
                await this.documentProcessingService.ReprocessDocumentByIdAsync(
                    storedDocument.Id, confirm: true);

            // then
            actualPlan.Started.Should().BeTrue();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(storedDocument.Id),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.ReprocessDocumentByIdAsync(storedDocument.Id),
                    Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task ShouldThrowValidationExceptionOnDeleteIfIdIsInvalidAsync(
            int invalidDocumentId)
        {
            // when
            Func<Task<DocumentRemovalPlan>> removeTask = async () =>
                await this.documentProcessingService.RemoveDocumentByIdAsync(
                    invalidDocumentId, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentServiceMock.Verify(service =>
                service.RemoveDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
        }
    }
}
