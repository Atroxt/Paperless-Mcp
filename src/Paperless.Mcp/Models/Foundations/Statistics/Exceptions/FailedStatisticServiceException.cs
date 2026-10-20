using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Statistics.Exceptions
{
    public class FailedStatisticServiceException : Xeption
    {
        public FailedStatisticServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
