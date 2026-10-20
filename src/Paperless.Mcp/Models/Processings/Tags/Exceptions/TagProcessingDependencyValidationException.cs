using Xeptions;

namespace Paperless.Mcp.Models.Processings.Tags.Exceptions
{
    public class TagProcessingDependencyValidationException : Xeption
    {
        public TagProcessingDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
