using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedStoragePathDependencyValidationException =
                new StoragePathDependencyValidationException(
                    message: "StoragePath dependency validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfConflictErrorOccursAndLogItAsync()
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();
            var httpResponseConflictException = new HttpResponseConflictException();

            var alreadyExistsStoragePathException =
                new AlreadyExistsStoragePathException(
                    message: "StoragePath already exists.",
                    innerException: httpResponseConflictException);

            var expectedStoragePathDependencyValidationException =
                new StoragePathDependencyValidationException(
                    message: "StoragePath dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsStoragePathException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(httpResponseConflictException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfStoragePathIsNotFoundAndLogItAsync()
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundStoragePathException =
                new NotFoundStoragePathException(
                    message: "StoragePath not found.",
                    innerException: httpResponseNotFoundException);

            var expectedStoragePathDependencyValidationException =
                new StoragePathDependencyValidationException(
                    message: "StoragePath dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundStoragePathException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(CriticalDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnModifyIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();

            var failedStoragePathDependencyException =
                new FailedStoragePathDependencyException(
                    message: "Failed storagePath dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedStoragePathDependencyException =
                new StoragePathDependencyException(
                    message: "StoragePath dependency error occurred, contact support.",
                    innerException: failedStoragePathDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnModifyIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();

            var failedStoragePathDependencyException =
                new FailedStoragePathDependencyException(
                    message: "Failed storagePath dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedStoragePathDependencyException =
                new StoragePathDependencyException(
                    message: "StoragePath dependency error occurred, contact support.",
                    innerException: failedStoragePathDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnModifyIfTransportErrorOccursAndLogItAsync()
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();
            var httpRequestException = new HttpRequestException();

            var failedStoragePathDependencyException =
                new FailedStoragePathDependencyException(
                    message: "Failed storagePath dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedStoragePathDependencyException =
                new StoragePathDependencyException(
                    message: "StoragePath dependency error occurred, contact support.",
                    innerException: failedStoragePathDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();
            var serviceException = new Exception();

            var failedStoragePathServiceException =
                new FailedStoragePathServiceException(
                    message: "Failed storagePath service error occurred, contact support.",
                    innerException: serviceException);

            var expectedStoragePathServiceException =
                new StoragePathServiceException(
                    message: "StoragePath service error occurred, contact support.",
                    innerException: failedStoragePathServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            StoragePath someStoragePath = CreateRandomStoragePath();

            var unauthorizedStoragePathException =
                new UnauthorizedStoragePathException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedStoragePathDependencyValidationException =
                new StoragePathDependencyValidationException(
                    message: "StoragePath dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedStoragePathException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<StoragePath>> modifyStoragePathAsyncTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(someStoragePath);

            // then
            await modifyStoragePathAsyncTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

    }
}
