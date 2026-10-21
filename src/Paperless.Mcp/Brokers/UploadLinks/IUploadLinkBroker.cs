using Paperless.Mcp.Models.Foundations.UploadLinks;

namespace Paperless.Mcp.Brokers.UploadLinks
{
    // Support-Broker fuer den Vorrat ausgestellter Upload-Links. Haelt sie im
    // Arbeitsspeicher und erzeugt die unraetbaren Kennungen.
    public interface IUploadLinkBroker
    {
        ValueTask<string> GenerateIdAsync();
        ValueTask StoreAsync(UploadLink uploadLink);

        // Entfernt den Link und gibt ihn zurueck. Ein zweiter Aufruf liefert null.
        ValueTask<UploadLink?> TakeAsync(string id);
        ValueTask RemoveExpiredAsync(DateTimeOffset now);
    }
}
