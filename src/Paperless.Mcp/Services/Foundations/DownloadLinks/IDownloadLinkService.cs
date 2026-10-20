using Paperless.Mcp.Models.Foundations.DownloadLinks;

namespace Paperless.Mcp.Services.Foundations.DownloadLinks
{
    public interface IDownloadLinkService
    {
        // Stellt einen Einmal-Link aus und gibt dessen Kennung zurueck. Die absolute
        // URL setzt der Exposer zusammen -- er kennt das Protokoll, der Dienst nicht.
        ValueTask<DownloadLink> IssueDownloadLinkAsync(
            int documentId,
            bool original,
            string token);

        // Loest einen Link ein. Beim zweiten Mal oder nach Ablauf schlaegt es fehl.
        ValueTask<DownloadLink> RedeemDownloadLinkAsync(string id);
    }
}
