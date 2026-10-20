using Xeptions;

namespace Paperless.Mcp.Models.Processings.Healths.Exceptions
{
    public class HealthProcessingServiceException : Xeption
    {
        public HealthProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
