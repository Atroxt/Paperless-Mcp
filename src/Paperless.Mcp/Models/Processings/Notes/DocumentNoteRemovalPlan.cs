namespace Paperless.Mcp.Models.Processings.Notes
{
    // Ergebnis eines Loeschvorgangs -- geplant oder ausgefuehrt.
    //
    // Destruktive Operationen laufen per Default als Trockenlauf: ohne confirm wird
    // nichts entfernt, sondern berichtet, was entfernt WUERDE. Der Bericht nennt den
    // Text der Notiz, damit eine Bestaetigung ueberhaupt etwas wert ist -- eine Notiz
    // loeschen zu wollen und die richtige Notiz zu loeschen ist nicht dasselbe.
    public class DocumentNoteRemovalPlan
    {
        public int DocumentId { get; set; }

        public int NoteId { get; set; }

        // Der Text der betroffenen Notiz, zur Vorlage gekuerzt.
        public string Note { get; set; } = string.Empty;

        // Wie viele Notizen das Dokument vor dem Vorgang trug.
        public int NoteCount { get; set; }

        // false bei einem Trockenlauf, true wenn tatsaechlich entfernt wurde.
        public bool Removed { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
