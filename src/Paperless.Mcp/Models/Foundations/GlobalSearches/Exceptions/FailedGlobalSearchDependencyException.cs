using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class FailedGlobalSearchDependencyException : Xeption
    {
        public FailedGlobalSearchDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
