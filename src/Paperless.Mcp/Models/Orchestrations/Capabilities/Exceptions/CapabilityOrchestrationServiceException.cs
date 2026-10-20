using Xeptions;

namespace Paperless.Mcp.Models.Orchestrations.Capabilities.Exceptions
{
    public class CapabilityOrchestrationServiceException : Xeption
    {
        public CapabilityOrchestrationServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
