using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class CorrespondentServiceException : Xeption
    {
        public CorrespondentServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
