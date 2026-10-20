using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class UnauthorizedTagException : Xeption
    {
        public UnauthorizedTagException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
