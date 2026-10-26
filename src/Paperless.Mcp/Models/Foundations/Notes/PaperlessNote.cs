namespace Paperless.Mcp.Models.Foundations.Notes
{
    // Drahtform des POST-Rumpfs: Paperless erwartet ausschliesslich den Text, das
    // Dokument steht im Pfad.
    public class PaperlessNote
    {
        public string Note { get; set; } = string.Empty;
    }
}
