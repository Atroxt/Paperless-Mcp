using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class FailedCorrespondentServiceException : Xeption
    {
        public FailedCorrespondentServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
