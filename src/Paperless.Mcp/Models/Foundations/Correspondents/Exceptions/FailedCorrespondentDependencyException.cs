using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class FailedCorrespondentDependencyException : Xeption
    {
        public FailedCorrespondentDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
