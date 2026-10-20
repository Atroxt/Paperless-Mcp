namespace Paperless.Mcp.Models.Foundations.Tasks
{
    // Drahtform von GET /api/tasks/?task_id=...
    //
    // Der Endpunkt ist paginiert, auch wenn nach genau einer Kennung gefiltert wird --
    // an der Instanz geprueft, nicht angenommen.
    public class PaperlessTaskPage
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<PaperlessTask> Results { get; set; } = [];
    }
}
