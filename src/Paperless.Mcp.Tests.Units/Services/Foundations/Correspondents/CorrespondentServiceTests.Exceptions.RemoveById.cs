using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveByIdIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int someCorrespondentId = GetRandomNumber();
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedCorrespondentDependencyValidationException =
                new CorrespondentDependencyValidationException(
                    message: "Correspondent dependency validation error occurred, fix the errors and try again.",
                    innerException: invalidCorrespondentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task> removeCorrespondentByIdTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(someCorrespondentId);

            // then
            await removeCorrespondentByIdTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveByIdIfCorrespondentIsNotFoundAndLogItAsync()
        {
            // given
            int someCorrespondentId = GetRandomNumber();
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundCorrespondentException =
                new NotFoundCorrespondentException(
                    message: "Correspondent not found.",
                    innerException: httpResponseNotFoundException);

            var expectedCorrespondentDependencyValidationException =
                new CorrespondentDependencyValidationException(
                    message: "Correspondent dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundCorrespondentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task> removeCorrespondentByIdTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(someCorrespondentId);

            // then
            await removeCorrespondentByIdTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyValidationException))),
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
            int someCorrespondentId = GetRandomNumber();

            var failedCorrespondentDependencyException =
                new FailedCorrespondentDependencyException(
                    message: "Failed correspondent dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedCorrespondentDependencyException =
                new CorrespondentDependencyException(
                    message: "Correspondent dependency error occurred, contact support.",
                    innerException: failedCorrespondentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task> removeCorrespondentByIdTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(someCorrespondentId);

            // then
            await removeCorrespondentByIdTask.Should()
                .ThrowAsync<CorrespondentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyException))),
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
            int someCorrespondentId = GetRandomNumber();

            var failedCorrespondentDependencyException =
                new FailedCorrespondentDependencyException(
                    message: "Failed correspondent dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedCorrespondentDependencyException =
                new CorrespondentDependencyException(
                    message: "Correspondent dependency error occurred, contact support.",
                    innerException: failedCorrespondentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task> removeCorrespondentByIdTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(someCorrespondentId);

            // then
            await removeCorrespondentByIdTask.Should()
                .ThrowAsync<CorrespondentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRemoveByIdIfTransportErrorOccursAndLogItAsync()
        {
            // given
            int someCorrespondentId = GetRandomNumber();
            var httpRequestException = new HttpRequestException();

            var failedCorrespondentDependencyException =
                new FailedCorrespondentDependencyException(
                    message: "Failed correspondent dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedCorrespondentDependencyException =
                new CorrespondentDependencyException(
                    message: "Correspondent dependency error occurred, contact support.",
                    innerException: failedCorrespondentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task> removeCorrespondentByIdTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(someCorrespondentId);

            // then
            await removeCorrespondentByIdTask.Should()
                .ThrowAsync<CorrespondentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRemoveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int someCorrespondentId = GetRandomNumber();
            var serviceException = new Exception();

            var failedCorrespondentServiceException =
                new FailedCorrespondentServiceException(
                    message: "Failed correspondent service error occurred, contact support.",
                    innerException: serviceException);

            var expectedCorrespondentServiceException =
                new CorrespondentServiceException(
                    message: "Correspondent service error occurred, contact support.",
                    innerException: failedCorrespondentServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task> removeCorrespondentByIdTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(someCorrespondentId);

            // then
            await removeCorrespondentByIdTask.Should()
                .ThrowAsync<CorrespondentServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentServiceException))),
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
            int someCorrespondentId = GetRandomNumber();

            var unauthorizedCorrespondentException =
                new UnauthorizedCorrespondentException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedCorrespondentDependencyValidationException =
                new CorrespondentDependencyValidationException(
                    message: "Correspondent dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedCorrespondentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task> removeCorrespondentByIdAsyncTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(someCorrespondentId);

            // then
            await removeCorrespondentByIdAsyncTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

    }
}
