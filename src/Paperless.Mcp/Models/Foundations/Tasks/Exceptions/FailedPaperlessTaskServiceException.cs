using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class FailedPaperlessTaskServiceException : Xeption
    {
        public FailedPaperlessTaskServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
