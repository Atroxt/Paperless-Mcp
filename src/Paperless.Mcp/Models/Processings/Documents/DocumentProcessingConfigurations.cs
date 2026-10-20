namespace Paperless.Mcp.Models.Processings.Documents
{
    // Configuration Model. Program.cs liest den Wert und reicht ihn herein -- die
    // Processing Service greift nicht selbst auf die Konfiguration zu.
    public class DocumentProcessingConfigurations
    {
        public int MaxPageSize { get; set; } = DefaultMaxPageSize;

        public const int DefaultMaxPageSize = 100;

        public const string MaxPageSizeVariable = "MAX_PAGE_SIZE";

        public const string MaxPageSizeKey = "Paperless:MaxPageSize";
    }
}
