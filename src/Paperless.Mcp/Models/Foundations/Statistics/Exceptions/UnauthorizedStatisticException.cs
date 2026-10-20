using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Statistics.Exceptions
{
    public class UnauthorizedStatisticException : Xeption
    {
        public UnauthorizedStatisticException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
