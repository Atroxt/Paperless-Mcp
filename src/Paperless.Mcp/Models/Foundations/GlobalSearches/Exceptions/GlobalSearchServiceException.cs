using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class GlobalSearchServiceException : Xeption
    {
        public GlobalSearchServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
