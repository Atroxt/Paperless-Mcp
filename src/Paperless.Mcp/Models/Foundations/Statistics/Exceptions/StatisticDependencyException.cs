using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Statistics.Exceptions
{
    public class StatisticDependencyException : Xeption
    {
        public StatisticDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
