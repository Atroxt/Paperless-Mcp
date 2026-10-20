using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    public class PaperlessTaskValidationException : Xeption
    {
        public PaperlessTaskValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
