namespace Paperless.Mcp.Models.Processings.Documents
{
    // Reprocess laesst Paperless die Datei erneut einlesen. Der erkannte Text wird dabei
    // neu erzeugt -- von Hand korrigierte Inhalte sind danach weg. Deshalb faellt es
    // unter dieselbe Bestaetigungspflicht wie das Loeschen.
    public class DocumentReprocessPlan
    {
        public int DocumentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool Started { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
