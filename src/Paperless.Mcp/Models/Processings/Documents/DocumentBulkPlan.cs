namespace Paperless.Mcp.Models.Processings.Documents
{
    // Der Bericht einer Sammeloperation. Bei einer Sammeloperation ist die Tragweite die
    // eigentliche Auskunft: "3 Dokumente" ist eine andere Entscheidung als "3 von 40
    // angefragten, die uebrigen 37 gibt es nicht".
    public class DocumentBulkPlan
    {
        public List<int> RequestedDocumentIds { get; set; } = [];

        // Die tatsaechlich vorhandenen davon.
        public List<int> AffectedDocumentIds { get; set; } = [];

        // Paperless laesst unbekannte Ids stillschweigend weg. Hier stehen sie, damit ein
        // Tippfehler sichtbar wird, statt als Erfolg durchzugehen.
        public List<int> UnknownDocumentIds { get; set; } = [];

        public List<string> AffectedTitles { get; set; } = [];

        // Was geaendert wuerde beziehungsweise wurde, je Eintrag ein Satz.
        public List<string> Changes { get; set; } = [];

        public bool Applied { get; set; }

        public string Summary { get; set; } = string.Empty;
    }
}
