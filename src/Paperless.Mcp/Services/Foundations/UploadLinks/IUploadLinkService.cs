using Paperless.Mcp.Models.Foundations.UploadLinks;

namespace Paperless.Mcp.Services.Foundations.UploadLinks
{
    public interface IUploadLinkService
    {
        // Stellt einen Einmal-Link aus. Die absolute URL setzt der Exposer zusammen.
        ValueTask<UploadLink> IssueUploadLinkAsync(UploadLink uploadLink);

        // Loest einen Link ein. Beim zweiten Mal oder nach Ablauf schlaegt es fehl.
        ValueTask<UploadLink> RedeemUploadLinkAsync(string id);
    }
}
