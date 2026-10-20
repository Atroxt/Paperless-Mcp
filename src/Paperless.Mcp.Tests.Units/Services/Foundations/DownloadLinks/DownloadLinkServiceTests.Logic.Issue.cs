using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.DownloadLinks;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DownloadLinks
{
    public partial class DownloadLinkServiceTests
    {
        [TestMethod]
        public async Task ShouldIssueDownloadLinkAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            int randomDocumentId = GetRandomNumber();
            string randomToken = GetRandomToken();
            string randomId = GetRandomId();

            var expectedDownloadLink = new DownloadLink
            {
                Id = randomId,
                DocumentId = randomDocumentId,
                Original = true,
                Token = randomToken,
                ExpiresAt = randomDateTimeOffset.AddSeconds(LifetimeSeconds),
            };

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.downloadLinkBrokerMock.Setup(broker =>
                broker.GenerateIdAsync())
                    .ReturnsAsync(randomId);

            // when
            DownloadLink actualDownloadLink =
                await this.downloadLinkService.IssueDownloadLinkAsync(
                    randomDocumentId, original: true, token: randomToken);

            // then
            actualDownloadLink.Should().BeEquivalentTo(expectedDownloadLink);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            // Aufraeumen gehoert zum Ausstellen: ohne das waechst der Vorrat abgelaufener
            // Eintraege unbegrenzt, denn einen Hintergrunddienst gibt es nicht.
            this.downloadLinkBrokerMock.Verify(broker =>
                broker.RemoveExpiredAsync(randomDateTimeOffset),
                    Times.Once);

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.GenerateIdAsync(),
                    Times.Once);

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.StoreAsync(It.Is<DownloadLink>(downloadLink =>
                    downloadLink.Id == expectedDownloadLink.Id
                    && downloadLink.DocumentId == expectedDownloadLink.DocumentId
                    && downloadLink.Original == expectedDownloadLink.Original
                    && downloadLink.Token == expectedDownloadLink.Token
                    && downloadLink.ExpiresAt == expectedDownloadLink.ExpiresAt)),
                        Times.Once);

            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
