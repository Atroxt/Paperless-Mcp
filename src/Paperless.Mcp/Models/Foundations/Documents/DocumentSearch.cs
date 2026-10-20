namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Virtuelles Modell: es gibt keine Paperless-Antwort dieser Form, sondern es buendelt
    // die Suchkriterien. Ein Vertragsmodell, sobald eine Methode sonst mehr als drei
    // primitive Eingaben haette.
    //
    // Fest typisiert und bewusst NICHT als freies Feld-Wert-Verzeichnis: Paperless
    // ignoriert unbekannte Abfrageparameter stillschweigend. Ein Tippfehler im Filter
    // liefert dann ungefiltert alles zurueck, statt zu scheitern -- bei einer Suche in
    // fremden Dokumenten ist das die gefaehrlichere Antwort.
    public class DocumentSearch
    {
        // Volltextsuche ueber Inhalt und Metadaten
        public string? Query { get; set; }

        // Teilstring im Titel, unabhaengig von Gross- und Kleinschreibung
        public string? TitleContains { get; set; }

        public int? CorrespondentId { get; set; }

        public int? DocumentTypeId { get; set; }

        public int? StoragePathId { get; set; }

        // Dokumente, die ALLE genannten Tags tragen
        public List<int> TagIds { get; set; } = [];

        // Genau diese Dokumente, ueber id__in. Paperless laesst unbekannte Ids dabei
        // stillschweigend weg -- wer die Liste zum Abgleich nutzt, muss selbst zaehlen.
        public List<int> DocumentIds { get; set; } = [];

        public DateOnly? CreatedFrom { get; set; }

        public DateOnly? CreatedTo { get; set; }

        // Paperless-Feldname, optional mit "-" fuer absteigend, etwa "-created"
        public string? Ordering { get; set; }

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = DefaultPageSize;

        public const int DefaultPageSize = 25;
    }
}
