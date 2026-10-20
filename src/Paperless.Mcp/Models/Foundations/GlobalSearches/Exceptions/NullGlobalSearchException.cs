using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    public class NullGlobalSearchException : Xeption
    {
        public NullGlobalSearchException(string message)
            : base(message)
        { }
    }
}
