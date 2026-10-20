using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Capabilities.Exceptions
{
    public class FailedCapabilityOrchestrationServiceException : Xeption
    {
        public FailedCapabilityOrchestrationServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
