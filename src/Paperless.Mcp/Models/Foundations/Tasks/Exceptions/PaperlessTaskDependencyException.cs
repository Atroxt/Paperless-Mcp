using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class PaperlessTaskDependencyException : Xeption
    {
        public PaperlessTaskDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
