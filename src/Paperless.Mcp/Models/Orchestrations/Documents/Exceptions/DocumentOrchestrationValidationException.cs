using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Documents.Exceptions
{
    public class DocumentOrchestrationValidationException : Xeption
    {
        public DocumentOrchestrationValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
