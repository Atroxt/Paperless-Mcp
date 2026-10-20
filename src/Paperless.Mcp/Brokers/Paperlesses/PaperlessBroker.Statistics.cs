using Paperless.Mcp.Models.Foundations.Statistics;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string StatisticsRelativeUrl = "api/statistics/";

        public async ValueTask<Statistic> GetStatisticAsync() =>
            await GetAsync<Statistic>(StatisticsRelativeUrl);
    }
}
