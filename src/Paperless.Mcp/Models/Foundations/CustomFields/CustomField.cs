namespace Paperless.Mcp.Models.Foundations.CustomFields
{
    // Primary Model. Deutlich schmaler als die anderen Entities: ein Custom Field hat
    // weder Slug noch Matching-Algorithmus noch Besitzer -- es ist eine Felddefinition,
    // kein Ordnungsmerkmal. Form nach GET /api/custom_fields/{id}/.
    public class CustomField
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Pflichtfeld: Paperless antwortet ohne data_type mit 400.
        public CustomFieldDataType DataType { get; set; }

        // Nur fuer bestimmte Datentypen belegt: Auswahloptionen bei Select,
        // Standardwaehrung bei Monetary. Sonst null.
        public CustomFieldExtraData? ExtraData { get; set; }

        // Von Paperless vergeben
        public int DocumentCount { get; set; }
    }
}
