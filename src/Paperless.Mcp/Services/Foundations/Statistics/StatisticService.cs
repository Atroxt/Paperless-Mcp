using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Statistics;

namespace Paperless.Mcp.Services.Foundations.Statistics
{
    public partial class StatisticService : IStatisticService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public StatisticService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Statistic> RetrieveStatisticAsync() =>
            TryCatch(async () => await this.paperlessBroker.GetStatisticAsync());
    }
}
