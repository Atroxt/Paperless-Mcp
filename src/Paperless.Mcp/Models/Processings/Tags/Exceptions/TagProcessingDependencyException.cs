using Xeptions;

namespace Paperless.Mcp.Models.Processings.Tags.Exceptions
{
    public class TagProcessingDependencyException : Xeption
    {
        public TagProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
