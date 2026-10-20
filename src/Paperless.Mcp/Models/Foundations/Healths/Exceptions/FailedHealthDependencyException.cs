using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Healths.Exceptions
{
    public class FailedHealthDependencyException : Xeption
    {
        public FailedHealthDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
