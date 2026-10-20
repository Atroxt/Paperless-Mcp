using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class FailedTagDependencyException : Xeption
    {
        public FailedTagDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
