namespace Paperless.Mcp.Models.Processings.CustomFields
{
    // Ergebnis eines Loeschvorgangs -- geplant oder ausgefuehrt.
    //
    // Destruktive Operationen laufen per Default als Trockenlauf: ohne confirm wird
    // nichts entfernt, sondern berichtet, was entfernt WUERDE. Der Bericht nennt die
    // Tragweite, damit eine Bestaetigung ueberhaupt etwas wert ist -- "haengt an 143
    // Dokumenten" ist eine andere Entscheidung als "haengt an keinem".
    public class CustomFieldRemovalPlan
    {
        public int CustomFieldId { get; set; }

        public string Name { get; set; } = string.Empty;

        // Wie viele Dokumente einen Wert fuer dieses Feld haben. Die Werte verschwinden
        // mit dem Feld, die Dokumente bleiben.
        public int DocumentCount { get; set; }

        // false bei einem Trockenlauf, true wenn tatsaechlich entfernt wurde.
        public bool Removed { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
