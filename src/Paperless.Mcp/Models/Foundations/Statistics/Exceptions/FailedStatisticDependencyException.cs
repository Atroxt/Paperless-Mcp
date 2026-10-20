using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Statistics.Exceptions
{
    public class FailedStatisticDependencyException : Xeption
    {
        public FailedStatisticDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
