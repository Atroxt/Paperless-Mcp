using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Orchestrations.Capabilities;
using Paperless.Mcp.Models.Orchestrations.Capabilities.Exceptions;
using Paperless.Mcp.Models.Processings.Healths;
using Paperless.Mcp.Models.Processings.Healths.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Orchestrations.Capabilities;
using Paperless.Mcp.Services.Orchestrations.Documents;
using Paperless.Mcp.Services.Processings.Healths;

namespace Paperless.Mcp.Tests.Units.Services.Orchestrations.Capabilities
{
    [TestClass]
    public class CapabilityOrchestrationServiceTests
    {
        private const int MaxPageSize = 100;
        private const int LinkLifetimeSeconds = 300;
        private const long MaxUploadLinkBytes = 200L * 1024 * 1024;

        private readonly Mock<IHealthProcessingService> healthProcessingServiceMock;

        public CapabilityOrchestrationServiceTests() =>
            this.healthProcessingServiceMock = new Mock<IHealthProcessingService>();

        private ICapabilityOrchestrationService CreateService(string transport) =>
            new CapabilityOrchestrationService(
                healthProcessingService: this.healthProcessingServiceMock.Object,
                configurations: new CapabilityConfigurations
                {
                    ServerVersion = "1.2.3",
                    Transport = transport,
                    LocalFileAccess = transport == CapabilityConfigurations.StdioTransport,
                    DownloadLinks = transport == CapabilityConfigurations.HttpTransport,
                    RequiresRequestToken = transport == CapabilityConfigurations.HttpTransport,
                    MaxPageSize = MaxPageSize,
                    DownloadLinkLifetimeSeconds = LinkLifetimeSeconds,
                    UploadLinks = transport == CapabilityConfigurations.HttpTransport,
                    MaxUploadLinkBytes = transport == CapabilityConfigurations.HttpTransport
                        ? MaxUploadLinkBytes
                        : 0,
                });

        [TestMethod]
        public async Task ShouldReportCapabilitiesOverHttpAsync()
        {
            // given
            var storedHealth = new PaperlessHealth
            {
                IsReachable = true,
                IsAuthenticated = true,
                Version = "v3.3.2",
            };

            this.healthProcessingServiceMock.Setup(service =>
                service.RetrieveHealthAsync())
                    .ReturnsAsync(storedHealth);

            ICapabilityOrchestrationService service =
                CreateService(CapabilityConfigurations.HttpTransport);

            // when
            ServerCapabilities actualCapabilities =
                await service.RetrieveCapabilitiesAsync();

            // then
            actualCapabilities.ServerVersion.Should().Be("1.2.3");
            actualCapabilities.Transport.Should().Be("http");
            actualCapabilities.Paperless.Should().BeEquivalentTo(storedHealth);
            actualCapabilities.LocalFileAccess.Should().BeFalse();
            actualCapabilities.DownloadLinks.Should().BeTrue();
            actualCapabilities.RequiresRequestToken.Should().BeTrue();
            actualCapabilities.MaxPageSize.Should().Be(MaxPageSize);
            actualCapabilities.DownloadLinkLifetimeSeconds.Should().Be(LinkLifetimeSeconds);
            actualCapabilities.UploadLinks.Should().BeTrue();
            actualCapabilities.MaxUploadLinkBytes.Should().Be(MaxUploadLinkBytes);

            // Die Grenzen kommen aus den Diensten, die sie durchsetzen -- nicht aus einer
            // zweiten Zahl, die irgendwann auseinanderliefe.
            actualCapabilities.MaxDocumentsPerBulkCall.Should()
                .Be(DocumentService.MaxDocumentsPerBulkCall);

            actualCapabilities.MaxUploadWaitSeconds.Should()
                .Be(DocumentOrchestrationService.MaxTimeoutSeconds);

            actualCapabilities.Summary.Should().Contain("v3.3.2");
            actualCapabilities.Summary.Should().Contain("download links");

            this.healthProcessingServiceMock.Verify(service =>
                service.RetrieveHealthAsync(),
                    Times.Once);

            this.healthProcessingServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportCapabilitiesOverStdioAsync()
        {
            // given
            this.healthProcessingServiceMock.Setup(service =>
                service.RetrieveHealthAsync())
                    .ReturnsAsync(new PaperlessHealth
                    {
                        IsReachable = true,
                        IsAuthenticated = true,
                        Version = "v3.3.2",
                    });

            ICapabilityOrchestrationService service =
                CreateService(CapabilityConfigurations.StdioTransport);

            // when
            ServerCapabilities actualCapabilities =
                await service.RetrieveCapabilitiesAsync();

            // then
            actualCapabilities.Transport.Should().Be("stdio");
            actualCapabilities.LocalFileAccess.Should().BeTrue();
            actualCapabilities.DownloadLinks.Should().BeFalse();
            actualCapabilities.RequiresRequestToken.Should().BeFalse();
            actualCapabilities.Summary.Should().Contain("local paths");

            this.healthProcessingServiceMock.Verify(service =>
                service.RetrieveHealthAsync(),
                    Times.Once);

            this.healthProcessingServiceMock.VerifyNoOtherCalls();
        }

        // Der Bericht ist gerade dann nuetzlich, wenn die Instanz nicht antwortet: die
        // Konfiguration dieses Servers gilt unabhaengig davon.
        [TestMethod]
        public async Task ShouldStillReportWhenPaperlessIsUnreachableAsync()
        {
            // given
            this.healthProcessingServiceMock.Setup(service =>
                service.RetrieveHealthAsync())
                    .ReturnsAsync(new PaperlessHealth
                    {
                        IsReachable = false,
                        Detail = "The Paperless instance did not answer.",
                    });

            ICapabilityOrchestrationService service =
                CreateService(CapabilityConfigurations.HttpTransport);

            // when
            ServerCapabilities actualCapabilities =
                await service.RetrieveCapabilitiesAsync();

            // then
            actualCapabilities.Paperless.IsReachable.Should().BeFalse();
            actualCapabilities.ServerVersion.Should().Be("1.2.3");
            actualCapabilities.Summary.Should().Contain("not");
            actualCapabilities.Summary.Should().Contain("reachable");

            this.healthProcessingServiceMock.Verify(service =>
                service.RetrieveHealthAsync(),
                    Times.Once);

            this.healthProcessingServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionIfHealthServiceFailsAsync()
        {
            // given
            this.healthProcessingServiceMock.Setup(service =>
                service.RetrieveHealthAsync())
                    .ThrowsAsync(new HealthProcessingServiceException(
                        message: "Health processing service error occurred, contact support.",
                        innerException: new FailedHealthProcessingServiceException(
                            message: "Failed health processing service error occurred, contact support.",
                            innerException: new Exception())));

            ICapabilityOrchestrationService service =
                CreateService(CapabilityConfigurations.HttpTransport);

            // when
            Func<Task<ServerCapabilities>> retrieveTask = async () =>
                await service.RetrieveCapabilitiesAsync();

            // then
            await retrieveTask.Should()
                .ThrowAsync<CapabilityOrchestrationServiceException>();

            this.healthProcessingServiceMock.Verify(service =>
                service.RetrieveHealthAsync(),
                    Times.Once);

            this.healthProcessingServiceMock.VerifyNoOtherCalls();
        }
    }
}
