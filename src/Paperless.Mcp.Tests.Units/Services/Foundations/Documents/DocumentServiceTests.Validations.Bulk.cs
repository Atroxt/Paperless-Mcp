using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        // Der wichtigste Validierungstest der Sammeloperationen: bulk_edit antwortet auf
        // eine leere Dokumentliste mit 200 OK, ohne etwas zu tun -- an der Instanz
        // geprueft. Ginge das durch, haette der Aufrufer einen Erfolg gemeldet bekommen,
        // bei dem nichts geschehen ist.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkIfDocumentIdsAreEmptyAndLogItAsync()
        {
            // given
            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(PaperlessBulkEdit.Documents),
                value: "At least one id is required");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task> removeDocumentsTask = async () =>
                await this.documentService.RemoveDocumentsAsync([]);

            // then
            await removeDocumentsTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.IsAny<PaperlessBulkEdit>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkIfADocumentIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(PaperlessBulkEdit.Documents),
                value: "Every id must be greater than zero");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task> removeDocumentsTask = async () =>
                await this.documentService.RemoveDocumentsAsync([GetRandomNumber(), 0]);

            // then
            await removeDocumentsTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.IsAny<PaperlessBulkEdit>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        // Die Obergrenze begrenzt die Tragweite eines einzelnen Aufrufs und haelt die
        // betroffenen Dokumente auf einer Seite nachlesbar.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkIfTooManyDocumentsAndLogItAsync()
        {
            // given
            List<int> tooManyIds = Enumerable
                .Range(1, DocumentService.MaxDocumentsPerBulkCall + 1)
                .ToList();

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(PaperlessBulkEdit.Documents),
                value: $"At most {DocumentService.MaxDocumentsPerBulkCall} documents per call");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task> removeDocumentsTask = async () =>
                await this.documentService.RemoveDocumentsAsync(tooManyIds);

            // then
            await removeDocumentsTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.IsAny<PaperlessBulkEdit>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnBulkIfReferenceIdIsInvalidAndLogItAsync(
            int invalidCorrespondentId)
        {
            // given
            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(Document.Correspondent),
                value: "Value must be greater than zero, or null to clear the reference");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task> modifyTask = async () =>
                await this.documentService.ModifyDocumentsCorrespondentAsync(
                    [GetRandomNumber()], invalidCorrespondentId);

            // then
            await modifyTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.IsAny<PaperlessBulkEdit>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyTagsIfNoTagChangesAndLogItAsync()
        {
            // given
            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(PaperlessBulkEdit.Parameters),
                value: "At least one tag must be added or removed");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task> modifyTagsTask = async () =>
                await this.documentService.ModifyDocumentsTagsAsync(
                    [GetRandomNumber()], addTagIds: [], removeTagIds: []);

            // then
            await modifyTagsTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.IsAny<PaperlessBulkEdit>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
