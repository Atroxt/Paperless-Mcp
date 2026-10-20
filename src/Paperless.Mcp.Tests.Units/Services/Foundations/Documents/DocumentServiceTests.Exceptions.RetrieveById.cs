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
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveByIdIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
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
                broker.GetDocumentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<Document>> retrieveDocumentByIdTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(someDocumentId);

            // then
            await retrieveDocumentByIdTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveByIdIfDocumentIsNotFoundAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
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
                broker.GetDocumentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<Document>> retrieveDocumentByIdTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(someDocumentId);

            // then
            await retrieveDocumentByIdTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int someDocumentId = GetRandomNumber();

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<Document>> retrieveDocumentByIdTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(someDocumentId);

            // then
            await retrieveDocumentByIdTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int someDocumentId = GetRandomNumber();

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<Document>> retrieveDocumentByIdTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(someDocumentId);

            // then
            await retrieveDocumentByIdTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfTransportErrorOccursAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
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
                broker.GetDocumentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<Document>> retrieveDocumentByIdTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(someDocumentId);

            // then
            await retrieveDocumentByIdTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
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
                broker.GetDocumentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<Document>> retrieveDocumentByIdTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(someDocumentId);

            // then
            await retrieveDocumentByIdTask.Should()
                .ThrowAsync<DocumentServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveByIdIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int someDocumentId = GetRandomNumber();

            var unauthorizedDocumentException =
                new UnauthorizedDocumentException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedDocumentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedDocumentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<Document>> retrieveDocumentByIdAsyncTask = async () =>
                await this.documentService.RetrieveDocumentByIdAsync(someDocumentId);

            // then
            await retrieveDocumentByIdAsyncTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(It.IsAny<int>()),
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
