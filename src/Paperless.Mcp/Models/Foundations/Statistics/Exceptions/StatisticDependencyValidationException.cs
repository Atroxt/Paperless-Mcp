using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Statistics.Exceptions
{
    public class StatisticDependencyValidationException : Xeption
    {
        public StatisticDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
