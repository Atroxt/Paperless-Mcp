namespace Paperless.Mcp.Models.Foundations.Tasks
{
    // Ein Verarbeitungsauftrag von Paperless. Nach einem Upload ist das der einzige
    // Faden zum entstehenden Dokument: RelatedDocumentIds fuellt sich, sobald die
    // Verarbeitung durch ist.
    //
    // Bewusst PaperlessTask und nicht Task: der Name waere sonst in jeder Datei mit
    // System.Threading.Tasks.Task verwechselbar.
    public class PaperlessTask
    {
        public int Id { get; set; }

        public string TaskId { get; set; } = string.Empty;

        public string TaskType { get; set; } = string.Empty;

        // Paperless liefert hier klein geschriebene Zeichenketten, etwa "success" --
        // an der Instanz geprueft, nicht die Grossschreibung aus Celery.
        public string Status { get; set; } = string.Empty;

        public string? StatusDisplay { get; set; }

        public DateTimeOffset? DateCreated { get; set; }

        public DateTimeOffset? DateDone { get; set; }

        public List<int> RelatedDocumentIds { get; set; } = [];
    }
}
