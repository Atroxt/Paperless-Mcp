using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.DownloadLinks;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;

namespace Paperless.Mcp.Services.Foundations.DownloadLinks
{
    public partial class DownloadLinkService : IDownloadLinkService
    {
        private readonly IDownloadLinkBroker downloadLinkBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly DownloadLinkConfigurations configurations;

        public DownloadLinkService(
            IDownloadLinkBroker downloadLinkBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker,
            DownloadLinkConfigurations configurations)
        {
            this.downloadLinkBroker = downloadLinkBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
            this.configurations = configurations;
        }

        public ValueTask<DownloadLink> IssueDownloadLinkAsync(
            int documentId,
            bool original,
            string token) =>
            TryCatch(async () =>
            {
                ValidateOnIssue(documentId, token);

                DateTimeOffset now = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                // Beim Ausstellen aufraeumen: so wachsen abgelaufene Eintraege nicht an,
                // ohne dass es dafuer einen Hintergrunddienst braucht.
                await this.downloadLinkBroker.RemoveExpiredAsync(now);

                var downloadLink = new DownloadLink
                {
                    Id = await this.downloadLinkBroker.GenerateIdAsync(),
                    DocumentId = documentId,
                    Original = original,
                    Token = token,
                    ExpiresAt = now.AddSeconds(this.configurations.LifetimeSeconds),
                };

                await this.downloadLinkBroker.StoreAsync(downloadLink);

                return downloadLink;
            });

        public ValueTask<DownloadLink> RedeemDownloadLinkAsync(string id) =>
            TryCatch(async () =>
            {
                ValidateId(id);

                // Nehmen statt lesen: damit ist der Link verbraucht, auch wenn er
                // abgelaufen war.
                DownloadLink? downloadLink = await this.downloadLinkBroker.TakeAsync(id);

                if (downloadLink is null)
                {
                    throw new NotFoundDownloadLinkException(
                        message: "Download link is unknown or was already used.");
                }

                DateTimeOffset now = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                return downloadLink.ExpiresAt > now
                    ? downloadLink
                    : throw new NotFoundDownloadLinkException(
                        message: "Download link has expired.");
            });
    }
}
