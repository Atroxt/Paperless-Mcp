using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Healths;
using Paperless.Mcp.Models.Foundations.Healths.Exceptions;
using Paperless.Mcp.Models.Processings.Healths;
using Paperless.Mcp.Models.Processings.Healths.Exceptions;
using Paperless.Mcp.Services.Foundations.Healths;
using Paperless.Mcp.Services.Processings.Healths;
using RESTFulSense.Exceptions;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Healths
{
    [TestClass]
    public class HealthProcessingServiceTests
    {
        private readonly Mock<IHealthService> healthServiceMock;
        private readonly IHealthProcessingService healthProcessingService;

        public HealthProcessingServiceTests()
        {
            this.healthServiceMock = new Mock<IHealthService>();

            this.healthProcessingService =
                new HealthProcessingService(healthService: this.healthServiceMock.Object);
        }

        [TestMethod]
        public async Task ShouldReportHealthyAsync()
        {
            // given
            var remoteVersion = new RemoteVersion
            {
                Version = "v3.3.2",
                UpdateAvailable = false,
            };

            this.healthServiceMock.Setup(service =>
                service.RetrieveRemoteVersionAsync())
                    .ReturnsAsync(remoteVersion);

            this.healthServiceMock.Setup(service =>
                service.RetrieveStatusAsync())
                    .ReturnsAsync(new PaperlessStatus());

            // when
            PaperlessHealth actualHealth =
                await this.healthProcessingService.RetrieveHealthAsync();

            // then
            actualHealth.IsReachable.Should().BeTrue();
            actualHealth.IsAuthenticated.Should().BeTrue();
            actualHealth.Version.Should().Be("v3.3.2");
            actualHealth.UpdateAvailable.Should().BeFalse();
            actualHealth.Detail.Should().BeNull();

            this.healthServiceMock.Verify(service =>
                service.RetrieveRemoteVersionAsync(),
                    Times.Once);

            this.healthServiceMock.Verify(service =>
                service.RetrieveStatusAsync(),
                    Times.Once);

            this.healthServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportUnreachableAndSkipTokenCheckAsync()
        {
            // given
            var healthDependencyException =
                new HealthDependencyException(
                    message: "Health dependency error occurred, contact support.",
                    innerException: new FailedHealthDependencyException(
                        message: "Failed health dependency error occurred, contact support.",
                        innerException: new HttpRequestException()));

            this.healthServiceMock.Setup(service =>
                service.RetrieveRemoteVersionAsync())
                    .ThrowsAsync(healthDependencyException);

            // when
            PaperlessHealth actualHealth =
                await this.healthProcessingService.RetrieveHealthAsync();

            // then
            actualHealth.IsReachable.Should().BeFalse();
            actualHealth.IsAuthenticated.Should().BeFalse();
            actualHealth.Detail.Should().Contain("PAPERLESS_BASE_URL");

            // Wenn der Server nicht antwortet, ist die Token-Pruefung sinnlos.
            this.healthServiceMock.Verify(service =>
                service.RetrieveRemoteVersionAsync(),
                    Times.Once);

            this.healthServiceMock.Verify(service =>
                service.RetrieveStatusAsync(),
                    Times.Never);

            this.healthServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportReachableButUnauthenticatedAsync()
        {
            // given
            var remoteVersion = new RemoteVersion { Version = "v3.3.2" };

            var healthDependencyValidationException =
                new HealthDependencyValidationException(
                    message: "Health dependency validation error occurred, fix the errors and try again.",
                    innerException: new UnauthorizedHealthException(
                        message: "Paperless rejected the API token.",
                        innerException: new HttpResponseUnauthorizedException()));

            this.healthServiceMock.Setup(service =>
                service.RetrieveRemoteVersionAsync())
                    .ReturnsAsync(remoteVersion);

            this.healthServiceMock.Setup(service =>
                service.RetrieveStatusAsync())
                    .ThrowsAsync(healthDependencyValidationException);

            // when
            PaperlessHealth actualHealth =
                await this.healthProcessingService.RetrieveHealthAsync();

            // then
            actualHealth.IsReachable.Should().BeTrue();
            actualHealth.IsAuthenticated.Should().BeFalse();
            actualHealth.Version.Should().Be("v3.3.2");
            actualHealth.Detail.Should().Contain("PAPERLESS_API_TOKEN");

            this.healthServiceMock.Verify(service =>
                service.RetrieveRemoteVersionAsync(),
                    Times.Once);

            this.healthServiceMock.Verify(service =>
                service.RetrieveStatusAsync(),
                    Times.Once);

            this.healthServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportReachableWhenStatusEndpointFailsAsync()
        {
            // given
            var remoteVersion = new RemoteVersion { Version = "v3.3.2" };

            var healthDependencyException =
                new HealthDependencyException(
                    message: "Health dependency error occurred, contact support.",
                    innerException: new FailedHealthDependencyException(
                        message: "Failed health dependency error occurred, contact support.",
                        innerException: new HttpResponseServiceUnavailableException()));

            this.healthServiceMock.Setup(service =>
                service.RetrieveRemoteVersionAsync())
                    .ReturnsAsync(remoteVersion);

            this.healthServiceMock.Setup(service =>
                service.RetrieveStatusAsync())
                    .ThrowsAsync(healthDependencyException);

            // when
            PaperlessHealth actualHealth =
                await this.healthProcessingService.RetrieveHealthAsync();

            // then
            actualHealth.IsReachable.Should().BeTrue();
            actualHealth.IsAuthenticated.Should().BeFalse();
            actualHealth.Detail.Should().Contain("starting up");

            this.healthServiceMock.Verify(service =>
                service.RetrieveRemoteVersionAsync(),
                    Times.Once);

            this.healthServiceMock.Verify(service =>
                service.RetrieveStatusAsync(),
                    Times.Once);

            this.healthServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowProcessingServiceExceptionOnUnexpectedErrorAsync()
        {
            // given
            var serviceException = new Exception();

            this.healthServiceMock.Setup(service =>
                service.RetrieveRemoteVersionAsync())
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<PaperlessHealth>> retrieveHealthTask = async () =>
                await this.healthProcessingService.RetrieveHealthAsync();

            // then
            await retrieveHealthTask.Should()
                .ThrowAsync<HealthProcessingServiceException>();

            this.healthServiceMock.Verify(service =>
                service.RetrieveRemoteVersionAsync(),
                    Times.Once);

            this.healthServiceMock.VerifyNoOtherCalls();
        }
    }
}
