using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class CorrespondentValidationException : Xeption
    {
        public CorrespondentValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
