namespace Paperless.Mcp.Models.Foundations.DownloadLinks
{
    // Ein ausgestellter Einmal-Link.
    //
    // Der Link selbst IST der Ausweis: wer ihn hat, bekommt die Datei, ohne ein
    // Paperless-Token mitzusenden. Das ist nur vertretbar, weil er unraetbar ist
    // (32 Zufallsbytes), nach kurzer Zeit verfaellt und genau einmal eingeloest werden
    // kann -- und weil er das Token DES AUFRUFERS festhaelt, nicht das des Betreibers.
    // Der Download laeuft also mit den Rechten dessen, der den Link angefordert hat.
    public class DownloadLink
    {
        public string Id { get; set; } = string.Empty;

        public int DocumentId { get; set; }

        // true liefert die Originaldatei, false die Archivfassung.
        public bool Original { get; set; }

        // Das Token des Aufrufers, mit dem die Datei spaeter geholt wird.
        public string Token { get; set; } = string.Empty;

        public DateTimeOffset ExpiresAt { get; set; }
    }
}
