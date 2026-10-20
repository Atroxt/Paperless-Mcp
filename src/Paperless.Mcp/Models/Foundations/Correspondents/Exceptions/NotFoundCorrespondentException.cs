using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class NotFoundCorrespondentException : Xeption
    {
        public NotFoundCorrespondentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
