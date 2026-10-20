using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class TagDependencyValidationException : Xeption
    {
        public TagDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
