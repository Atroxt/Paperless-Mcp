using Xeptions;

namespace Paperless.Mcp.Models.Processings.Tags.Exceptions
{
    public class TagProcessingValidationException : Xeption
    {
        public TagProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
