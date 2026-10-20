using Paperless.Mcp.Models.Foundations.Statistics;

namespace Paperless.Mcp.Services.Foundations.Statistics
{
    public interface IStatisticService
    {
        ValueTask<Statistic> RetrieveStatisticAsync();
    }
}
