namespace Paperless.Mcp.Models.Foundations.CustomFields
{
    // Secondary Model: eine einzelne Auswahlmoeglichkeit eines Select-Feldes.
    public class CustomFieldSelectOption
    {
        public string Label { get; set; } = string.Empty;

        // Von Paperless vergeben, beim Anlegen leer lassen.
        public string? Id { get; set; }
    }
}
