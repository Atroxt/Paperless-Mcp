using Paperless.Mcp.Models.Foundations.Statistics;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<Statistic> GetStatisticAsync();
    }
}
