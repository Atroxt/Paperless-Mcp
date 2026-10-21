using System.Collections.Concurrent;
using System.Security.Cryptography;
using Paperless.Mcp.Models.Foundations.UploadLinks;

namespace Paperless.Mcp.Brokers.UploadLinks
{
    // Als Singleton registriert, bewusst nur im Arbeitsspeicher -- siehe
    // DownloadLinkBroker.
    public class UploadLinkBroker : IUploadLinkBroker
    {
        private readonly ConcurrentDictionary<string, UploadLink> links = new();

        public async ValueTask<string> GenerateIdAsync() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        public async ValueTask StoreAsync(UploadLink uploadLink) =>
            this.links[uploadLink.Id] = uploadLink;

        public async ValueTask<UploadLink?> TakeAsync(string id) =>
            this.links.TryRemove(id, out UploadLink? uploadLink)
                ? uploadLink
                : null;

        public async ValueTask RemoveExpiredAsync(DateTimeOffset now)
        {
            foreach ((string id, UploadLink uploadLink) in this.links)
            {
                if (uploadLink.ExpiresAt <= now)
                {
                    this.links.TryRemove(id, out _);
                }
            }
        }
    }
}
