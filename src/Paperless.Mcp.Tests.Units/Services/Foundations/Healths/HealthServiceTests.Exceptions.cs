using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Healths;
using Paperless.Mcp.Models.Foundations.Healths.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Healths
{
    public partial class HealthServiceTests
    {
        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveStatusIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            var unauthorizedHealthException =
                new UnauthorizedHealthException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedHealthDependencyValidationException =
                new HealthDependencyValidationException(
                    message: "Health dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedHealthException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatusAsync())
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<PaperlessStatus>> retrieveStatusTask = async () =>
                await this.healthService.RetrieveStatusAsync();

            // then
            await retrieveStatusTask.Should()
                .ThrowAsync<HealthDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedHealthDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatusAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedHealthDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrieveRemoteVersionIfServerIsUnreachableAndLogItAsync()
        {
            // given
            var httpRequestException = new HttpRequestException();

            var failedHealthDependencyException =
                new FailedHealthDependencyException(
                    message: "Failed health dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedHealthDependencyException =
                new HealthDependencyException(
                    message: "Health dependency error occurred, contact support.",
                    innerException: failedHealthDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetRemoteVersionAsync())
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<RemoteVersion>> retrieveRemoteVersionTask = async () =>
                await this.healthService.RetrieveRemoteVersionAsync();

            // then
            await retrieveRemoteVersionTask.Should()
                .ThrowAsync<HealthDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedHealthDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetRemoteVersionAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedHealthDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrieveRemoteVersionIfEndpointIsMissingAndLogItAsync()
        {
            // given
            var httpResponseUrlNotFoundException = new HttpResponseUrlNotFoundException();

            var failedHealthDependencyException =
                new FailedHealthDependencyException(
                    message: "Failed health dependency error occurred, contact support.",
                    innerException: httpResponseUrlNotFoundException);

            var expectedHealthDependencyException =
                new HealthDependencyException(
                    message: "Health dependency error occurred, contact support.",
                    innerException: failedHealthDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetRemoteVersionAsync())
                    .ThrowsAsync(httpResponseUrlNotFoundException);

            // when
            Func<Task<RemoteVersion>> retrieveRemoteVersionTask = async () =>
                await this.healthService.RetrieveRemoteVersionAsync();

            // then
            await retrieveRemoteVersionTask.Should()
                .ThrowAsync<HealthDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedHealthDependencyException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedHealthDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetRemoteVersionAsync(),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrieveStatusIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            var failedHealthDependencyException =
                new FailedHealthDependencyException(
                    message: "Failed health dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedHealthDependencyException =
                new HealthDependencyException(
                    message: "Health dependency error occurred, contact support.",
                    innerException: failedHealthDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatusAsync())
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<PaperlessStatus>> retrieveStatusTask = async () =>
                await this.healthService.RetrieveStatusAsync();

            // then
            await retrieveStatusTask.Should()
                .ThrowAsync<HealthDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedHealthDependencyException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedHealthDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatusAsync(),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrieveRemoteVersionIfServiceErrorOccursAndLogItAsync()
        {
            // given
            var serviceException = new Exception();

            var failedHealthServiceException =
                new FailedHealthServiceException(
                    message: "Failed health service error occurred, contact support.",
                    innerException: serviceException);

            var expectedHealthServiceException =
                new HealthServiceException(
                    message: "Health service error occurred, contact support.",
                    innerException: failedHealthServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetRemoteVersionAsync())
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<RemoteVersion>> retrieveRemoteVersionTask = async () =>
                await this.healthService.RetrieveRemoteVersionAsync();

            // then
            await retrieveRemoteVersionTask.Should()
                .ThrowAsync<HealthServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedHealthServiceException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedHealthServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetRemoteVersionAsync(),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
