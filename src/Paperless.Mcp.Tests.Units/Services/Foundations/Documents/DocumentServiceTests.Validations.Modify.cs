using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyIfUpdateIsNullAndLogItAsync()
        {
            // given
            DocumentUpdate? nullUpdate = null;

            var nullDocumentException =
                new NullDocumentException(message: "DocumentUpdate is null.");

            var expectedValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: nullDocumentException);

            // when
            Func<Task<Document>> modifyTask = async () =>
                await this.documentService.ModifyDocumentAsync(nullUpdate!);

            // then
            await modifyTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnModifyIfIdIsInvalidAndLogItAsync(
            int invalidDocumentId)
        {
            // given
            var invalidUpdate = new DocumentUpdate
            {
                DocumentId = invalidDocumentId,
                Title = "egal",
            };

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentUpdate.DocumentId),
                value: "Value must be greater than zero");

            var expectedValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<Document>> modifyTask = async () =>
                await this.documentService.ModifyDocumentAsync(invalidUpdate);

            // then
            await modifyTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        // Eine Aenderung ohne Aenderung waere ein stiller Leerlauf: der Aufrufer bekaeme
        // das unveraenderte Dokument und hielte es fuer einen Erfolg.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyIfNothingWouldChangeAndLogItAsync()
        {
            // given
            var emptyUpdate = new DocumentUpdate { DocumentId = GetRandomNumber() };

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentUpdate),
                value: "At least one field must be set");

            var expectedValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<Document>> modifyTask = async () =>
                await this.documentService.ModifyDocumentAsync(emptyUpdate);

            // then
            await modifyTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyIfCustomFieldIdIsInvalidAndLogItAsync()
        {
            // given
            var invalidUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                CustomFields = [new DocumentCustomFieldValue { Field = 0, Value = "egal" }],
            };

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentUpdate.CustomFields),
                value: "Every custom field value must reference a field id greater than zero");

            var expectedValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<Document>> modifyTask = async () =>
                await this.documentService.ModifyDocumentAsync(invalidUpdate);

            // then
            await modifyTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
