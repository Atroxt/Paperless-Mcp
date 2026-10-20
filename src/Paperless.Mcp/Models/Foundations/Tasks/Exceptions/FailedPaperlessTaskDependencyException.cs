using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class FailedPaperlessTaskDependencyException : Xeption
    {
        public FailedPaperlessTaskDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
