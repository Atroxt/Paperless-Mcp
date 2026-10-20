using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class TagDependencyException : Xeption
    {
        public TagDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
