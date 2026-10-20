using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class CorrespondentDependencyValidationException : Xeption
    {
        public CorrespondentDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
