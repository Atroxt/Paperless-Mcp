using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class NotFoundTagException : Xeption
    {
        public NotFoundTagException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
