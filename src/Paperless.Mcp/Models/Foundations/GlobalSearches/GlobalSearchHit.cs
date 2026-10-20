namespace Paperless.Mcp.Models.Foundations.GlobalSearches
{
    // Ein Treffer, bewusst leicht: Id und Bezeichnung, mehr nicht.
    //
    // Paperless liefert in der globalen Suche vollstaendige Objekte zurueck -- bei
    // Dokumenten einschliesslich des gesamten extrahierten Textes. Fuer einen
    // Assistenten waeren das viele Token fuer wenig Nutzen; mit Id und Bezeichnung kann
    // er gezielt das passende _get-Tool nachschieben.
    public class GlobalSearchHit
    {
        public int Id { get; set; }

        public string Label { get; set; } = string.Empty;
    }
}
