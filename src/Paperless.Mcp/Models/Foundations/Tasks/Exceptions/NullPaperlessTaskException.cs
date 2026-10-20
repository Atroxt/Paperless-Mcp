using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class NullPaperlessTaskException : Xeption
    {
        public NullPaperlessTaskException(string message)
            : base(message)
        { }
    }
}
