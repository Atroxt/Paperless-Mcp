using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class FailedTagServiceException : Xeption
    {
        public FailedTagServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
