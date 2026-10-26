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
        public async Task ShouldThrowValidationExceptionOnAddIfNoteIsNullAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();

            var nullDocumentNoteException =
                new NullDocumentNoteException(message: "Document note is null.");

            var expectedDocumentNoteValidationException =
                new DocumentNoteValidationException(
                    message: "Document note validation error occurred, fix the errors and try again.",
                    innerException: nullDocumentNoteException);

            // when
            Func<Task<List<DocumentNote>>> addDocumentNoteTask = async () =>
                await this.documentNoteService.AddDocumentNoteAsync(someDocumentId, note: null!);

            // then
            await addDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentNoteValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentNoteValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentNoteAsync(It.IsAny<int>(), It.IsAny<PaperlessNote>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnAddIfNoteIsInvalidAndLogItAsync(
            string invalidNote)
        {
            // given
            int someDocumentId = GetRandomNumber();

            var invalidDocumentNoteException =
                new InvalidDocumentNoteException(
                    message: "Document note is invalid, fix the errors and try again.");

            invalidDocumentNoteException.UpsertDataList(
                key: nameof(DocumentNote.Note),
                value: "Text is required");

            var expectedDocumentNoteValidationException =
                new DocumentNoteValidationException(
                    message: "Document note validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentNoteException);

            // when
            Func<Task<List<DocumentNote>>> addDocumentNoteTask = async () =>
                await this.documentNoteService.AddDocumentNoteAsync(someDocumentId, invalidNote);

            // then
            await addDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentNoteValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentNoteValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentNoteAsync(It.IsAny<int>(), It.IsAny<PaperlessNote>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        // Continuous validation: ein ungueltiges Dokument und ein leerer Text ergeben
        // zwei Eintraege auf einmal, nicht zwei Anlaeufe.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfDocumentIdAndNoteAreInvalidAndLogItAsync()
        {
            // given
            const int invalidDocumentId = 0;
            string invalidNote = string.Empty;

            var invalidDocumentNoteException =
                new InvalidDocumentNoteException(
                    message: "Document note is invalid, fix the errors and try again.");

            invalidDocumentNoteException.UpsertDataList(
                key: "DocumentId",
                value: "Value must be greater than zero");

            invalidDocumentNoteException.UpsertDataList(
                key: nameof(DocumentNote.Note),
                value: "Text is required");

            var expectedDocumentNoteValidationException =
                new DocumentNoteValidationException(
                    message: "Document note validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentNoteException);

            // when
            Func<Task<List<DocumentNote>>> addDocumentNoteTask = async () =>
                await this.documentNoteService.AddDocumentNoteAsync(
                    invalidDocumentId,
                    invalidNote);

            // then
            await addDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentNoteValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentNoteValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentNoteAsync(It.IsAny<int>(), It.IsAny<PaperlessNote>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfNoteExceedsMaximumLengthAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
            string tooLongNote = new('a', 10_001);

            var invalidDocumentNoteException =
                new InvalidDocumentNoteException(
                    message: "Document note is invalid, fix the errors and try again.");

            invalidDocumentNoteException.UpsertDataList(
                key: nameof(DocumentNote.Note),
                value: "Text exceeds the maximum length of 10000 characters");

            var expectedDocumentNoteValidationException =
                new DocumentNoteValidationException(
                    message: "Document note validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentNoteException);

            // when
            Func<Task<List<DocumentNote>>> addDocumentNoteTask = async () =>
                await this.documentNoteService.AddDocumentNoteAsync(someDocumentId, tooLongNote);

            // then
            await addDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentNoteValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentNoteValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentNoteAsync(It.IsAny<int>(), It.IsAny<PaperlessNote>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
