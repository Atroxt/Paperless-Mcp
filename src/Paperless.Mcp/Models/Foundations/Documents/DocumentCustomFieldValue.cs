namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Secondary Model: ein Custom-Field-Wert an einem Dokument, Form {field, value}.
    public class DocumentCustomFieldValue
    {
        // Id der Felddefinition, siehe Models/Foundations/CustomFields
        public int Field { get; set; }

        // Der Typ haengt an der Felddefinition: Zeichenkette bei string und monetary
        // ("EUR123.45"), Zahl bei integer und float, Wahrheitswert bei boolean, Datum
        // als "2026-10-19". Deshalb object statt eines festen Typs -- System.Text.Json
        // liest hier ein JsonElement und schreibt es unveraendert zurueck.
        public object? Value { get; set; }
    }
}
