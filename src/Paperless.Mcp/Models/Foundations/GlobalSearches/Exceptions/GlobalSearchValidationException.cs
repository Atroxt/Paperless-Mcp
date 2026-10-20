using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class GlobalSearchValidationException : Xeption
    {
        public GlobalSearchValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
