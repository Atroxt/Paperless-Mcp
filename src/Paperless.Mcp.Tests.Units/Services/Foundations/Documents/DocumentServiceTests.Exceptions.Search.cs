using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnSearchIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            DocumentSearch someSearch = CreateRandomDocumentSearch();
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedDocumentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<DocumentPage>> retrieveDocumentPageTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(someSearch);

            // then
            await retrieveDocumentPageTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Paperless antwortet mit 404, wenn die angefragte Seite ausserhalb des Bereichs liegt.
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnSearchIfPageIsOutOfRangeAndLogItAsync()
        {
            // given
            DocumentSearch someSearch = CreateRandomDocumentSearch();
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundDocumentException =
                new NotFoundDocumentException(
                    message: "Document not found.",
                    innerException: httpResponseNotFoundException);

            var expectedDocumentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundDocumentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<DocumentPage>> retrieveDocumentPageTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(someSearch);

            // then
            await retrieveDocumentPageTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(CriticalDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnSearchIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            DocumentSearch someSearch = CreateRandomDocumentSearch();

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<DocumentPage>> retrieveDocumentPageTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(someSearch);

            // then
            await retrieveDocumentPageTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnSearchIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            DocumentSearch someSearch = CreateRandomDocumentSearch();

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<DocumentPage>> retrieveDocumentPageTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(someSearch);

            // then
            await retrieveDocumentPageTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnSearchIfTransportErrorOccursAndLogItAsync()
        {
            // given
            DocumentSearch someSearch = CreateRandomDocumentSearch();
            var httpRequestException = new HttpRequestException();

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<DocumentPage>> retrieveDocumentPageTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(someSearch);

            // then
            await retrieveDocumentPageTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnSearchIfServiceErrorOccursAndLogItAsync()
        {
            // given
            DocumentSearch someSearch = CreateRandomDocumentSearch();
            var serviceException = new Exception();

            var failedDocumentServiceException =
                new FailedDocumentServiceException(
                    message: "Failed document service error occurred, contact support.",
                    innerException: serviceException);

            var expectedDocumentServiceException =
                new DocumentServiceException(
                    message: "Document service error occurred, contact support.",
                    innerException: failedDocumentServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<DocumentPage>> retrieveDocumentPageTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(someSearch);

            // then
            await retrieveDocumentPageTask.Should()
                .ThrowAsync<DocumentServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnSearchIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            DocumentSearch someSearch = CreateRandomDocumentSearch();

            var unauthorizedDocumentException =
                new UnauthorizedDocumentException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedDocumentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedDocumentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<DocumentPage>> retrieveDocumentPageAsyncTask = async () =>
                await this.documentService.RetrieveDocumentPageAsync(someSearch);

            // then
            await retrieveDocumentPageAsyncTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

    }
}
