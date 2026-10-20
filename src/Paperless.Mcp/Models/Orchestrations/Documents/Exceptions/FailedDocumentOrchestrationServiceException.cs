using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Documents.Exceptions
{
    public class FailedDocumentOrchestrationServiceException : Xeption
    {
        public FailedDocumentOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
