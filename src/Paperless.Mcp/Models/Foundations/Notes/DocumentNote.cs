namespace Paperless.Mcp.Models.Foundations.Notes
{
    // Primary Model: physisch selbststaendig, anemisch und flach.
    // Die snake_case-Namen der Paperless-API sind Technologiesprache und gehoeren in den
    // Broker, nicht ins Modell -- deshalb hier keine JsonPropertyName-Attribute.
    //
    // Der Typ heisst DocumentNote statt Note, weil die Eigenschaft, die den Text traegt,
    // bei Paperless "note" heisst und ein Member nicht so heissen darf wie sein Typ.
    public class DocumentNote
    {
        public int Id { get; set; }

        public string Note { get; set; } = string.Empty;

        public DateTimeOffset Created { get; set; }

        // Wer die Notiz angelegt hat. Paperless liefert je nach Version eine Id oder ein
        // Objekt, der Broker vereinheitlicht das auf die Id.
        public int? User { get; set; }

        // Nicht Teil der Antwort: das Dokument steht im Pfad. Die Foundation Service
        // traegt es nach, damit eine einzelne Notiz fuer sich sprechen kann.
        public int Document { get; set; }
    }
}
