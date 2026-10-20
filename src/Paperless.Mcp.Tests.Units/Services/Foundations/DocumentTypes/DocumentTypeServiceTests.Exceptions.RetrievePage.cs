using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedDocumentTypeDependencyValidationException =
                new DocumentTypeDependencyValidationException(
                    message: "DocumentType dependency validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentTypeException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(somePage, somePageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Paperless antwortet mit 404, wenn die angefragte Seite ausserhalb des Bereichs liegt.
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfPageIsOutOfRangeAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundDocumentTypeException =
                new NotFoundDocumentTypeException(
                    message: "DocumentType not found.",
                    innerException: httpResponseNotFoundException);

            var expectedDocumentTypeDependencyValidationException =
                new DocumentTypeDependencyValidationException(
                    message: "DocumentType dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundDocumentTypeException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(somePage, somePageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(CriticalDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var failedDocumentTypeDependencyException =
                new FailedDocumentTypeDependencyException(
                    message: "Failed documentType dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedDocumentTypeDependencyException =
                new DocumentTypeDependencyException(
                    message: "DocumentType dependency error occurred, contact support.",
                    innerException: failedDocumentTypeDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(somePage, somePageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var failedDocumentTypeDependencyException =
                new FailedDocumentTypeDependencyException(
                    message: "Failed documentType dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDocumentTypeDependencyException =
                new DocumentTypeDependencyException(
                    message: "DocumentType dependency error occurred, contact support.",
                    innerException: failedDocumentTypeDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(somePage, somePageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfTransportErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpRequestException = new HttpRequestException();

            var failedDocumentTypeDependencyException =
                new FailedDocumentTypeDependencyException(
                    message: "Failed documentType dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedDocumentTypeDependencyException =
                new DocumentTypeDependencyException(
                    message: "DocumentType dependency error occurred, contact support.",
                    innerException: failedDocumentTypeDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(somePage, somePageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrievePageIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var serviceException = new Exception();

            var failedDocumentTypeServiceException =
                new FailedDocumentTypeServiceException(
                    message: "Failed documentType service error occurred, contact support.",
                    innerException: serviceException);

            var expectedDocumentTypeServiceException =
                new DocumentTypeServiceException(
                    message: "DocumentType service error occurred, contact support.",
                    innerException: failedDocumentTypeServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(somePage, somePageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var unauthorizedDocumentTypeException =
                new UnauthorizedDocumentTypeException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedDocumentTypeDependencyValidationException =
                new DocumentTypeDependencyValidationException(
                    message: "DocumentType dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedDocumentTypeException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageAsyncTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(somePage, somePageSize);

            // then
            await retrieveDocumentTypePageAsyncTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

    }
}
