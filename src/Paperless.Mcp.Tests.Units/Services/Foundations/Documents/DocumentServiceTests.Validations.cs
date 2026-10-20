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
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync(
            int invalidDocumentId)
        {
            // given
            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(Document.Id),
                value: "Value must be greater than zero");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<Document>> retrieveTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(invalidDocumentId);

            // then
            await retrieveTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnSearchIfSearchIsNullAndLogItAsync()
        {
            // given
            DocumentSearch? nullSearch = null;

            var nullDocumentException =
                new NullDocumentException(message: "DocumentSearch is null.");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: nullDocumentException);

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(nullSearch!);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnSearchIfPageIsInvalidAndLogItAsync(
            int invalidPage)
        {
            // given
            DocumentSearch invalidSearch = CreateRandomDocumentSearch();
            invalidSearch.Page = invalidPage;

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentSearch.Page),
                value: "Value must be greater than zero");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(invalidSearch);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        // Ein unbekanntes Sortierfeld ignoriert Paperless stillschweigend. Die Antwort
        // waere dann anders sortiert als angefragt, ohne Hinweis -- deshalb hier die
        // geschlossene Liste.
        [TestMethod]
        [DataRow("gibtsnicht")]
        [DataRow("-gibtsnicht")]
        [DataRow("content")]
        public async Task ShouldThrowValidationExceptionOnSearchIfOrderingIsUnknownAndLogItAsync(
            string invalidOrdering)
        {
            // given
            DocumentSearch invalidSearch = CreateRandomDocumentSearch();
            invalidSearch.Ordering = invalidOrdering;

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentSearch.Ordering),
                value: "Ordering must be one of added, archive_serial_number, correspondent__name, "
                    + "created, document_type__name, id, modified, title, optionally prefixed with - for descending");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(invalidSearch);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnSearchIfDateRangeIsReversedAndLogItAsync()
        {
            // given
            DocumentSearch invalidSearch = CreateRandomDocumentSearch();
            invalidSearch.CreatedFrom = new DateOnly(2026, 6, 1);
            invalidSearch.CreatedTo = new DateOnly(2026, 1, 1);

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentSearch.CreatedFrom),
                value: "CreatedFrom must not be later than CreatedTo");

            var expectedDocumentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(invalidSearch);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
