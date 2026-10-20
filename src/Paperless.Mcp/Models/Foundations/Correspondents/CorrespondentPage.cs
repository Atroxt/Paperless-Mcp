namespace Paperless.Mcp.Models.Foundations.Correspondents
{
    public class CorrespondentPage
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<Correspondent> Results { get; set; } = [];
    }
}
