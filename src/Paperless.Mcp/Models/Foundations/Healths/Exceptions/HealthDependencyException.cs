using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Healths.Exceptions
{
    public class HealthDependencyException : Xeption
    {
        public HealthDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
