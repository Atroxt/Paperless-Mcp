using Xeptions;

namespace Paperless.Mcp.Models.Processings.Tags.Exceptions
{
    public class NullTagProcessingException : Xeption
    {
        public NullTagProcessingException(string message)
            : base(message)
        { }
    }
}
