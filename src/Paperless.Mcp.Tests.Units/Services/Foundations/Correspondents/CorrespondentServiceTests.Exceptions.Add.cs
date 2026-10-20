using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;
using RESTFulSense.Exceptions;

using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();
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
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfConflictErrorOccursAndLogItAsync()
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();
            var httpResponseConflictException = new HttpResponseConflictException();

            var alreadyExistsCorrespondentException =
                new AlreadyExistsCorrespondentException(
                    message: "Correspondent already exists.",
                    innerException: httpResponseConflictException);

            var expectedCorrespondentDependencyValidationException =
                new CorrespondentDependencyValidationException(
                    message: "Correspondent dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsCorrespondentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(httpResponseConflictException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfNotFoundErrorOccursAndLogItAsync()
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();
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
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
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
        public async Task ShouldThrowDependencyExceptionOnAddIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();

            var failedCorrespondentDependencyException =
                new FailedCorrespondentDependencyException(
                    message: "Failed correspondent dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedCorrespondentDependencyException =
                new CorrespondentDependencyException(
                    message: "Correspondent dependency error occurred, contact support.",
                    innerException: failedCorrespondentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
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
        public async Task ShouldThrowDependencyExceptionOnAddIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();

            var failedCorrespondentDependencyException =
                new FailedCorrespondentDependencyException(
                    message: "Failed correspondent dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedCorrespondentDependencyException =
                new CorrespondentDependencyException(
                    message: "Correspondent dependency error occurred, contact support.",
                    innerException: failedCorrespondentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnAddIfTransportErrorOccursAndLogItAsync()
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();
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
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();
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
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            Correspondent someCorrespondent = CreateRandomCorrespondent();

            var unauthorizedCorrespondentException =
                new UnauthorizedCorrespondentException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedCorrespondentDependencyValidationException =
                new CorrespondentDependencyValidationException(
                    message: "Correspondent dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedCorrespondentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<Correspondent>> addCorrespondentAsyncTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(someCorrespondent);

            // then
            await addCorrespondentAsyncTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
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
