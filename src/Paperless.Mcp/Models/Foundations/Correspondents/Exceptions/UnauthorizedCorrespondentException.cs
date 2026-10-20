using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class UnauthorizedCorrespondentException : Xeption
    {
        public UnauthorizedCorrespondentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
