using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class UnauthorizedPaperlessTaskException : Xeption
    {
        public UnauthorizedPaperlessTaskException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
