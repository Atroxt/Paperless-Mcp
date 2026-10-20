using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class GlobalSearchDependencyException : Xeption
    {
        public GlobalSearchDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
