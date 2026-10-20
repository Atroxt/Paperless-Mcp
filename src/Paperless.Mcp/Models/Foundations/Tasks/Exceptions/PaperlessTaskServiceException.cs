using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class PaperlessTaskServiceException : Xeption
    {
        public PaperlessTaskServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
