namespace Paperless.Mcp.Models.Foundations.Tags
{
    // Primary Model: physisch selbststaendig, anemisch und flach.
    // Die snake_case-Namen der Paperless-API sind Technologiesprache und gehoeren in den
    // Broker, nicht ins Modell -- deshalb hier keine JsonPropertyName-Attribute.
    public class Tag
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }

        public string? Match { get; set; }

        public TagMatchingAlgorithm MatchingAlgorithm { get; set; }

        public bool IsInsensitive { get; set; }

        public bool IsInboxTag { get; set; }

        public int? Owner { get; set; }

        // Von Paperless vergeben, beim Schreiben ignoriert
        public string Slug { get; set; } = string.Empty;

        public string? TextColor { get; set; }

        public int DocumentCount { get; set; }
    }
}
