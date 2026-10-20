using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Healths.Exceptions
{
    public class HealthDependencyValidationException : Xeption
    {
        public HealthDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
