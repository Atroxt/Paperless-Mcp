using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class NullTagException : Xeption
    {
        public NullTagException(string message)
            : base(message)
        { }
    }
}
