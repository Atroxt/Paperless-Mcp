using AwesomeAssertions;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Healths;
using RESTFulSense.Exceptions;

namespace Paperless.Mcp.Tests.Integrations.Brokers
{
    [TestClass]
    public class PaperlessBrokerHealthTests
    {
        [TestMethod]
        public async Task ShouldRetrieveRemoteVersionWithoutTokenAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            PaperlessBroker broker = IntegrationTestConfiguration.CreateBroker(
                apiTokenOverride: string.Empty);

            // when
            RemoteVersion actualRemoteVersion = await broker.GetRemoteVersionAsync();

            // then
            // Dieser Endpunkt antwortet ohne Token -- genau deshalb taugt er als
            // Erreichbarkeitstest, der auch bei falschem Token noch etwas aussagt.
            actualRemoteVersion.Should().NotBeNull();
            actualRemoteVersion.Version.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task ShouldRetrieveStatusWithValidTokenAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            PaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();

            // when
            PaperlessStatus actualStatus = await broker.GetStatusAsync();

            // then
            // Belegt zugleich, dass die snake_case-Zuordnung greift: das Feld heisst
            // pngx_version in der Antwort und PngxVersion im Modell.
            actualStatus.Should().NotBeNull();
            actualStatus.PngxVersion.Should().NotBeNullOrWhiteSpace();
        }

        [TestMethod]
        public async Task ShouldRejectInvalidTokenAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            PaperlessBroker broker = IntegrationTestConfiguration.CreateBroker(
                apiTokenOverride: "dieses-token-ist-ungueltig");

            // when
            Func<Task> retrieveStatusTask = async () =>
                await broker.GetStatusAsync();

            // then
            await retrieveStatusTask.Should()
                .ThrowAsync<HttpResponseUnauthorizedException>();
        }
    }
}
