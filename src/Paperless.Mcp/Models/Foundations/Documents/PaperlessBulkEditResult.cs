namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Paperless antwortet auf bulk_edit mit {"result":"OK"}.
    public class PaperlessBulkEditResult
    {
        public string Result { get; set; } = string.Empty;
    }
}
