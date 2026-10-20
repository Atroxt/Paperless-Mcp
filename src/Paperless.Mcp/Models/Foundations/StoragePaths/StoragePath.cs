namespace Paperless.Mcp.Models.Foundations.StoragePaths
{
    // Primary Model, anemisch und flach. Felder nach der echten Antwort von
    // GET /api/storage_paths/{id}/ -- wie ein Dokumenttyp, zusaetzlich mit Path.
    public class StoragePath
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Pflichtfeld: Paperless antwortet ohne Path mit 400. Enthaelt Platzhalter wie
        // {created_year}/{correspondent}/{title}, die Paperless beim Speichern in seine
        // eigene Schreibweise ({{ created_year }}) normalisiert -- die Antwort ist
        // deshalb nicht zeichengleich mit dem, was gesendet wurde.
        public string Path { get; set; } = string.Empty;

        public string? Match { get; set; }

        public StoragePathMatchingAlgorithm MatchingAlgorithm { get; set; }

        public bool IsInsensitive { get; set; }

        public int? Owner { get; set; }

        // Von Paperless vergeben, beim Schreiben ignoriert
        public string Slug { get; set; } = string.Empty;

        public int DocumentCount { get; set; }

        public bool UserCanChange { get; set; }
    }
}
