namespace Paperless.Mcp.Models.Foundations.Documents
{
    public class DocumentPage
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<Document> Results { get; set; } = [];
    }
}
