using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Orchestrations.Capabilities;
using Paperless.Mcp.Services.Foundations.Healths;
using Paperless.Mcp.Services.Orchestrations.Capabilities;
using Paperless.Mcp.Services.Processings.Healths;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Der Bericht ueber den Server selbst, gegen die echte Instanz: nur hier zeigt sich,
    // ob die Paperless-Version wirklich ankommt und nicht bloss ein leeres Feld.
    [TestClass]
    public class CapabilityTests
    {
        [TestMethod]
        public async Task ShouldReportTheRealPaperlessVersionAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = new LoggingBroker(new NullLogger<LoggingBroker>());

            var capabilityService = new CapabilityOrchestrationService(
                healthProcessingService: new HealthProcessingService(
                    new HealthService(broker, loggingBroker)),

                configurations: new CapabilityConfigurations
                {
                    ServerVersion = "0.0.0-test",
                    Transport = CapabilityConfigurations.HttpTransport,
                    DownloadLinks = true,
                    RequiresRequestToken = true,
                    MaxPageSize = 100,
                    DownloadLinkLifetimeSeconds = 300,
                });

            // when
            ServerCapabilities actualCapabilities =
                await capabilityService.RetrieveCapabilitiesAsync();

            // then
            actualCapabilities.Paperless.IsReachable.Should().BeTrue();
            actualCapabilities.Paperless.IsAuthenticated.Should().BeTrue();
            actualCapabilities.Paperless.Version.Should().NotBeNullOrWhiteSpace();

            // Die Grenzen stammen aus den Diensten, die sie durchsetzen.
            actualCapabilities.MaxDocumentsPerBulkCall.Should().Be(100);
            actualCapabilities.MaxUploadWaitSeconds.Should().Be(120);

            actualCapabilities.Summary.Should()
                .Contain(actualCapabilities.Paperless.Version);
        }
    }
}
