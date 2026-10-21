using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.UploadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions;

namespace Paperless.Mcp.Services.Foundations.UploadLinks
{
    public partial class UploadLinkService : IUploadLinkService
    {
        private readonly IUploadLinkBroker uploadLinkBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;
        private readonly UploadLinkConfigurations configurations;

        public UploadLinkService(
            IUploadLinkBroker uploadLinkBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker,
            UploadLinkConfigurations configurations)
        {
            this.uploadLinkBroker = uploadLinkBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
            this.configurations = configurations;
        }

        public ValueTask<UploadLink> IssueUploadLinkAsync(UploadLink uploadLink) =>
            TryCatch(async () =>
            {
                ValidateOnIssue(uploadLink);

                DateTimeOffset now = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                // Beim Ausstellen aufraeumen, wie beim Download-Link.
                await this.uploadLinkBroker.RemoveExpiredAsync(now);

                uploadLink.Id = await this.uploadLinkBroker.GenerateIdAsync();
                uploadLink.ExpiresAt = now.AddSeconds(this.configurations.LifetimeSeconds);

                await this.uploadLinkBroker.StoreAsync(uploadLink);

                return uploadLink;
            });

        public ValueTask<UploadLink> RedeemUploadLinkAsync(string id) =>
            TryCatch(async () =>
            {
                ValidateId(id);

                // Nehmen statt lesen: damit ist der Link verbraucht, auch wenn er
                // abgelaufen war.
                UploadLink? uploadLink = await this.uploadLinkBroker.TakeAsync(id);

                if (uploadLink is null)
                {
                    throw new NotFoundUploadLinkException(
                        message: "Upload link is unknown or was already used.");
                }

                DateTimeOffset now = await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                return uploadLink.ExpiresAt > now
                    ? uploadLink
                    : throw new NotFoundUploadLinkException(
                        message: "Upload link has expired.");
            });
    }
}
