namespace Paperless.Mcp.Models.Foundations.CustomFields
{
    public class CustomFieldPage
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<CustomField> Results { get; set; } = [];
    }
}
