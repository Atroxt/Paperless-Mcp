using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class UnauthorizedGlobalSearchException : Xeption
    {
        public UnauthorizedGlobalSearchException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
