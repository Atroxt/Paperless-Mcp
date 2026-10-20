using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Statistics.Exceptions
{
    public class StatisticServiceException : Xeption
    {
        public StatisticServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
