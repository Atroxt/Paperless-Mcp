namespace Paperless.Mcp.Models.Foundations.StoragePaths
{
    public class StoragePathPage
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<StoragePath> Results { get; set; } = [];
    }
}
