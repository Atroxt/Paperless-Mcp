using Xeptions;

namespace Paperless.Mcp.Models.Processings.Healths.Exceptions
{
    public class FailedHealthProcessingServiceException : Xeption
    {
        public FailedHealthProcessingServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
