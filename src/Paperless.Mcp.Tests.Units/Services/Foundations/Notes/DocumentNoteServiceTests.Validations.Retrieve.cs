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
        public async Task ShouldThrowValidationExceptionOnRetrieveIfDocumentIdIsInvalidAndLogItAsync(
            int invalidDocumentId)
        {
            // given
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
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(invalidDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentNoteValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentNoteValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
