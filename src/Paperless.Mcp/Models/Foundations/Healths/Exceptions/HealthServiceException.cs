using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Healths.Exceptions
{
    public class HealthServiceException : Xeption
    {
        public HealthServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
