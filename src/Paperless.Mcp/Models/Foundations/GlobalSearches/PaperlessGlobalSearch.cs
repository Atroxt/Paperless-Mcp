namespace Paperless.Mcp.Models.Foundations.GlobalSearches
{
    // Drahtform von GET /api/search/?query=...
    //
    // Nur die Felder, die wir auswerten: System.Text.Json ignoriert den Rest, sodass die
    // vollstaendigen Objekte der Antwort gar nicht erst entstehen. Die Umsetzung in die
    // Geschaeftsform uebernimmt die Foundation Service.
    public class PaperlessGlobalSearch
    {
        public int Total { get; set; }

        public List<TitledHit> Documents { get; set; } = [];

        public List<NamedHit> Tags { get; set; } = [];

        public List<NamedHit> Correspondents { get; set; } = [];

        public List<NamedHit> DocumentTypes { get; set; } = [];

        public List<NamedHit> StoragePaths { get; set; } = [];

        public List<NamedHit> CustomFields { get; set; } = [];

        // Dokumente heissen bei Paperless "title", alles andere "name". Das ist der
        // einzige Unterschied zwischen den Sammlungen.
        public class TitledHit
        {
            public int Id { get; set; }

            public string Title { get; set; } = string.Empty;
        }

        public class NamedHit
        {
            public int Id { get; set; }

            public string Name { get; set; } = string.Empty;
        }
    }
}
