namespace Paperless.Mcp.Models.Foundations.GlobalSearches
{
    // Geschaeftsform des Suchergebnisses.
    //
    // Paperless liefert zwoelf Sammlungen; hier stehen nur die sechs fachlichen. Nicht
    // uebernommen sind users, groups, mail_accounts, mail_rules, workflows und
    // saved_views: das ist Verwaltung, teils heikel, und ein Benutzerverzeichnis gehoert
    // nicht ungefragt in die Antwort eines Dokumenten-Werkzeugs.
    public class GlobalSearchResult
    {
        // Was Paperless insgesamt gezaehlt hat -- auch ueber die Sammlungen hinweg, die
        // hier nicht auftauchen. Die Summe der Listen unten kann also kleiner sein.
        public int Total { get; set; }

        public List<GlobalSearchHit> Documents { get; set; } = [];

        public List<GlobalSearchHit> Tags { get; set; } = [];

        public List<GlobalSearchHit> Correspondents { get; set; } = [];

        public List<GlobalSearchHit> DocumentTypes { get; set; } = [];

        public List<GlobalSearchHit> StoragePaths { get; set; } = [];

        public List<GlobalSearchHit> CustomFields { get; set; } = [];
    }
}
