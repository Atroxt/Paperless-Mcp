using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Documents.Exceptions
{
    public class DocumentOrchestrationDependencyException : Xeption
    {
        public DocumentOrchestrationDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
