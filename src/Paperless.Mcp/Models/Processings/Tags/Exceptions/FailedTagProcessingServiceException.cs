using Xeptions;

namespace Paperless.Mcp.Models.Processings.Tags.Exceptions
{
    public class FailedTagProcessingServiceException : Xeption
    {
        public FailedTagProcessingServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
