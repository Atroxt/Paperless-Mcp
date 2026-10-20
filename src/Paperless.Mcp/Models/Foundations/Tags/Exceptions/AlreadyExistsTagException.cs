using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class AlreadyExistsTagException : Xeption
    {
        public AlreadyExistsTagException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
