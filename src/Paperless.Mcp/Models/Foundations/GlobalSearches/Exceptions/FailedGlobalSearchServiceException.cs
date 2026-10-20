using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class FailedGlobalSearchServiceException : Xeption
    {
        public FailedGlobalSearchServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
