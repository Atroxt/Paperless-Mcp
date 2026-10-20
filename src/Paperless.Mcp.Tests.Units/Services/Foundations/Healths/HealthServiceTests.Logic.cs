using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Healths;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Healths
{
    public partial class HealthServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveRemoteVersionAsync()
        {
            // given
            RemoteVersion randomRemoteVersion = CreateRandomRemoteVersion();
            RemoteVersion storedRemoteVersion = randomRemoteVersion;
            RemoteVersion expectedRemoteVersion = storedRemoteVersion.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetRemoteVersionAsync())
                    .ReturnsAsync(storedRemoteVersion);

            // when
            RemoteVersion actualRemoteVersion =
                await this.healthService.RetrieveRemoteVersionAsync();

            // then
            actualRemoteVersion.Should().BeEquivalentTo(expectedRemoteVersion);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetRemoteVersionAsync(),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldRetrieveStatusAsync()
        {
            // given
            PaperlessStatus randomStatus = CreateRandomStatus();
            PaperlessStatus storedStatus = randomStatus;
            PaperlessStatus expectedStatus = storedStatus.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatusAsync())
                    .ReturnsAsync(storedStatus);

            // when
            PaperlessStatus actualStatus = await this.healthService.RetrieveStatusAsync();

            // then
            actualStatus.Should().BeEquivalentTo(expectedStatus);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatusAsync(),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
