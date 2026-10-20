using Paperless.Mcp.Models.Foundations.DownloadLinks;

namespace Paperless.Mcp.Brokers.DownloadLinks
{
    // Support-Broker fuer den Vorrat ausgestellter Links. Haelt sie im Arbeitsspeicher
    // und erzeugt die unraetbaren Kennungen -- beides Technologie, keine Fachlichkeit.
    public interface IDownloadLinkBroker
    {
        ValueTask<string> GenerateIdAsync();
        ValueTask StoreAsync(DownloadLink downloadLink);

        // Entfernt den Link und gibt ihn zurueck. Ein zweiter Aufruf liefert null --
        // daher "einmalig".
        ValueTask<DownloadLink?> TakeAsync(string id);
        ValueTask RemoveExpiredAsync(DateTimeOffset now);
    }
}
