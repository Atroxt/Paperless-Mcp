using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Healths.Exceptions
{
    public class UnauthorizedHealthException : Xeption
    {
        public UnauthorizedHealthException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
