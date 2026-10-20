using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class NotFoundPaperlessTaskException : Xeption
    {
        public NotFoundPaperlessTaskException(string message)
            : base(message)
        { }
    }
}
