namespace Paperless.Mcp.Models.Foundations.DocumentTypes
{
    public class DocumentTypePage
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<DocumentType> Results { get; set; } = [];
    }
}
