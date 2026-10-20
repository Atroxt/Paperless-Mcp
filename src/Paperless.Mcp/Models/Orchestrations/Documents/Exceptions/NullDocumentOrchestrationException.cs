using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Documents.Exceptions
{
    public class NullDocumentOrchestrationException : Xeption
    {
        public NullDocumentOrchestrationException(string message)
            : base(message)
        { }
    }
}
