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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();
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
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfConflictErrorOccursAndLogItAsync()
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();
            var httpResponseConflictException = new HttpResponseConflictException();

            var alreadyExistsDocumentTypeException =
                new AlreadyExistsDocumentTypeException(
                    message: "DocumentType already exists.",
                    innerException: httpResponseConflictException);

            var expectedDocumentTypeDependencyValidationException =
                new DocumentTypeDependencyValidationException(
                    message: "DocumentType dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsDocumentTypeException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(httpResponseConflictException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfNotFoundErrorOccursAndLogItAsync()
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();
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
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
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
        public async Task ShouldThrowDependencyExceptionOnAddIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();

            var failedDocumentTypeDependencyException =
                new FailedDocumentTypeDependencyException(
                    message: "Failed documentType dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedDocumentTypeDependencyException =
                new DocumentTypeDependencyException(
                    message: "DocumentType dependency error occurred, contact support.",
                    innerException: failedDocumentTypeDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
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
        public async Task ShouldThrowDependencyExceptionOnAddIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();

            var failedDocumentTypeDependencyException =
                new FailedDocumentTypeDependencyException(
                    message: "Failed documentType dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDocumentTypeDependencyException =
                new DocumentTypeDependencyException(
                    message: "DocumentType dependency error occurred, contact support.",
                    innerException: failedDocumentTypeDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnAddIfTransportErrorOccursAndLogItAsync()
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();
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
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();
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
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            DocumentType someDocumentType = CreateRandomDocumentType();

            var unauthorizedDocumentTypeException =
                new UnauthorizedDocumentTypeException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedDocumentTypeDependencyValidationException =
                new DocumentTypeDependencyValidationException(
                    message: "DocumentType dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedDocumentTypeException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<DocumentType>> addDocumentTypeAsyncTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(someDocumentType);

            // then
            await addDocumentTypeAsyncTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
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
