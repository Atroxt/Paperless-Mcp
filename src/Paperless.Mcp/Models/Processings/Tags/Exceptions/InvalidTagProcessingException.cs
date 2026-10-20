using Xeptions;

namespace Paperless.Mcp.Models.Processings.Tags.Exceptions
{
    public class InvalidTagProcessingException : Xeption
    {
        public InvalidTagProcessingException(string message)
            : base(message)
        { }
    }
}
