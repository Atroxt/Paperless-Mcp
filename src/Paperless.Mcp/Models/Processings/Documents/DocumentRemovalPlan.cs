namespace Paperless.Mcp.Models.Processings.Documents
{
    // Anders als bei den Ordnungsmerkmalen gibt es hier keine Tragweite ueber andere
    // Objekte: das Dokument selbst verschwindet, samt Datei und erkanntem Text.
    public class DocumentRemovalPlan
    {
        public int DocumentId { get; set; }

        public string Title { get; set; } = string.Empty;

        public bool Removed { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
