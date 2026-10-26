using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Foundations.Notes.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Notes
{
    public partial class DocumentNoteServiceTests
    {
        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRemoveIfNoteIdIsInvalidAndLogItAsync(
            int invalidNoteId)
        {
            // given
            int someDocumentId = GetRandomNumber();

            var invalidDocumentNoteException =
                new InvalidDocumentNoteException(
                    message: "Document note is invalid, fix the errors and try again.");

            invalidDocumentNoteException.UpsertDataList(
                key: nameof(DocumentNote.Id),
                value: "Value must be greater than zero");

            var expectedDocumentNoteValidationException =
                new DocumentNoteValidationException(
                    message: "Document note validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentNoteException);

            // when
            Func<Task<List<DocumentNote>>> removeDocumentNoteTask = async () =>
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    someDocumentId,
                    invalidNoteId);

            // then
            await removeDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentNoteValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentNoteValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRemoveIfDocumentIdIsInvalidAndLogItAsync(
            int invalidDocumentId)
        {
            // given
            int someNoteId = GetRandomNumber();

            var invalidDocumentNoteException =
                new InvalidDocumentNoteException(
                    message: "Document note is invalid, fix the errors and try again.");

            invalidDocumentNoteException.UpsertDataList(
                key: "DocumentId",
                value: "Value must be greater than zero");

            var expectedDocumentNoteValidationException =
                new DocumentNoteValidationException(
                    message: "Document note validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentNoteException);

            // when
            Func<Task<List<DocumentNote>>> removeDocumentNoteTask = async () =>
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    invalidDocumentId,
                    someNoteId);

            // then
            await removeDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentNoteValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentNoteValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
