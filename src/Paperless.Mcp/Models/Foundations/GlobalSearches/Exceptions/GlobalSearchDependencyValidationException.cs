using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class GlobalSearchDependencyValidationException : Xeption
    {
        public GlobalSearchDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
