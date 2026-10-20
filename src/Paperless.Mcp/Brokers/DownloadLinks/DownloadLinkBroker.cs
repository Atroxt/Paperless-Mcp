using System.Collections.Concurrent;
using System.Security.Cryptography;
using Paperless.Mcp.Models.Foundations.DownloadLinks;

namespace Paperless.Mcp.Brokers.DownloadLinks
{
    // Als Singleton registriert: der Vorrat muss Anfragen ueberdauern.
    //
    // Bewusst nur im Arbeitsspeicher. Ein Neustart entwertet alle offenen Links, und
    // mehrere Instanzen hinter einem Lastverteiler teilen sie nicht. Bei einer
    // Lebensdauer von Minuten ist das verschmerzbar und allemal besser, als Tokens in
    // eine Datenbank zu schreiben.
    public class DownloadLinkBroker : IDownloadLinkBroker
    {
        private readonly ConcurrentDictionary<string, DownloadLink> links = new();

        // 32 Zufallsbytes aus einem kryptografischen Generator, base64url-kodiert.
        // Raten scheidet damit aus, und die Kennung passt in eine URL.
        public async ValueTask<string> GenerateIdAsync() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

        public async ValueTask StoreAsync(DownloadLink downloadLink) =>
            this.links[downloadLink.Id] = downloadLink;

        public async ValueTask<DownloadLink?> TakeAsync(string id) =>
            this.links.TryRemove(id, out DownloadLink? downloadLink)
                ? downloadLink
                : null;

        public async ValueTask RemoveExpiredAsync(DateTimeOffset now)
        {
            foreach ((string id, DownloadLink downloadLink) in this.links)
            {
                if (downloadLink.ExpiresAt <= now)
                {
                    this.links.TryRemove(id, out _);
                }
            }
        }
    }
}
