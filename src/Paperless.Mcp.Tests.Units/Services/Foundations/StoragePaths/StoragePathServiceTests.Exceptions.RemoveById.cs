using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveByIdIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int someStoragePathId = GetRandomNumber();
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
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task> removeStoragePathByIdTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(someStoragePathId);

            // then
            await removeStoragePathByIdTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveByIdIfStoragePathIsNotFoundAndLogItAsync()
        {
            // given
            int someStoragePathId = GetRandomNumber();
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
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task> removeStoragePathByIdTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(someStoragePathId);

            // then
            await removeStoragePathByIdTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRemoveByIdIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int someStoragePathId = GetRandomNumber();

            var failedStoragePathDependencyException =
                new FailedStoragePathDependencyException(
                    message: "Failed storagePath dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedStoragePathDependencyException =
                new StoragePathDependencyException(
                    message: "StoragePath dependency error occurred, contact support.",
                    innerException: failedStoragePathDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task> removeStoragePathByIdTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(someStoragePathId);

            // then
            await removeStoragePathByIdTask.Should()
                .ThrowAsync<StoragePathDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRemoveByIdIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int someStoragePathId = GetRandomNumber();

            var failedStoragePathDependencyException =
                new FailedStoragePathDependencyException(
                    message: "Failed storagePath dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedStoragePathDependencyException =
                new StoragePathDependencyException(
                    message: "StoragePath dependency error occurred, contact support.",
                    innerException: failedStoragePathDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task> removeStoragePathByIdTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(someStoragePathId);

            // then
            await removeStoragePathByIdTask.Should()
                .ThrowAsync<StoragePathDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRemoveByIdIfTransportErrorOccursAndLogItAsync()
        {
            // given
            int someStoragePathId = GetRandomNumber();
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
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task> removeStoragePathByIdTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(someStoragePathId);

            // then
            await removeStoragePathByIdTask.Should()
                .ThrowAsync<StoragePathDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedStoragePathDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRemoveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int someStoragePathId = GetRandomNumber();
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
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> removeStoragePathByIdTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(someStoragePathId);

            // then
            await removeStoragePathByIdTask.Should()
                .ThrowAsync<StoragePathServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveByIdIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int someStoragePathId = GetRandomNumber();

            var unauthorizedStoragePathException =
                new UnauthorizedStoragePathException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedStoragePathDependencyValidationException =
                new StoragePathDependencyValidationException(
                    message: "StoragePath dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedStoragePathException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task> removeStoragePathByIdAsyncTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(someStoragePathId);

            // then
            await removeStoragePathByIdAsyncTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
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
