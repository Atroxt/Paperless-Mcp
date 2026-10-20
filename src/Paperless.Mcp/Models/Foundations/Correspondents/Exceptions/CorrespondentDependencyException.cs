using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class CorrespondentDependencyException : Xeption
    {
        public CorrespondentDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
