namespace Paperless.Mcp.Models.Foundations.Tags
{
    // Aggregation des Primary Models fuer die seitenweise Abfrage.
    // Bewusst eigen statt eines geteilten PaginatedResult<T>: geteilte Modelle ueber
    // unabhaengige Flows hinweg gibt es hier nicht, Duplizierung erhaelt Autonomie.
    public class TagPage
    {
        public int Count { get; set; }

        public string? Next { get; set; }

        public string? Previous { get; set; }

        public List<Tag> Results { get; set; } = [];
    }
}
