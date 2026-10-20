using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Documents.Exceptions
{
    public class InvalidDocumentOrchestrationException : Xeption
    {
        public InvalidDocumentOrchestrationException(string message)
            : base(message)
        { }
    }
}
