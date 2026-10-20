using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class PaperlessTaskDependencyValidationException : Xeption
    {
        public PaperlessTaskDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
