using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class TagValidationException : Xeption
    {
        public TagValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
