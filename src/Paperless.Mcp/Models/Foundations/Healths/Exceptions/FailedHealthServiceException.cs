using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Healths.Exceptions
{
    public class FailedHealthServiceException : Xeption
    {
        public FailedHealthServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
