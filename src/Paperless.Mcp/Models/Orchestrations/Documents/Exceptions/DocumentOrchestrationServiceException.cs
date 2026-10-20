using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Documents.Exceptions
{
    public class DocumentOrchestrationServiceException : Xeption
    {
        public DocumentOrchestrationServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
