using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.DownloadLinks;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DownloadLinks
{
    public partial class DownloadLinkServiceTests
    {
        [TestMethod]
        public async Task ShouldRedeemDownloadLinkAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            DownloadLink randomDownloadLink =
                CreateRandomDownloadLink(randomDateTimeOffset.AddSeconds(1));

            DownloadLink storedDownloadLink = randomDownloadLink.DeepClone();
            DownloadLink expectedDownloadLink = storedDownloadLink.DeepClone();
            string inputId = storedDownloadLink.Id;

            this.downloadLinkBrokerMock.Setup(broker =>
                broker.TakeAsync(inputId))
                    .ReturnsAsync(storedDownloadLink);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            DownloadLink actualDownloadLink =
                await this.downloadLinkService.RedeemDownloadLinkAsync(inputId);

            // then
            actualDownloadLink.Should().BeEquivalentTo(expectedDownloadLink);

            // Nehmen, nicht lesen: damit ist der Link nach diesem Aufruf verbraucht.
            this.downloadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(inputId),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
