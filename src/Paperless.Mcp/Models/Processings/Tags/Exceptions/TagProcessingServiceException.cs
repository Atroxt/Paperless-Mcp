using Xeptions;

namespace Paperless.Mcp.Models.Processings.Tags.Exceptions
{
    public class TagProcessingServiceException : Xeption
    {
        public TagProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
