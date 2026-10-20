using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Statistics;
using Paperless.Mcp.Services.Foundations.Statistics;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Prueft gegen eine echte Instanz die JSON-Form von /api/statistics/: nur lesend,
    // deshalb ohne Aufraeumen.
    [TestClass]
    public class StatisticServiceTests
    {
        private static IStatisticService CreateStatisticService()
        {
            IPaperlessBroker paperlessBroker = IntegrationTestConfiguration.CreateBroker();

            ILoggingBroker loggingBroker =
                new LoggingBroker(new NullLogger<LoggingBroker>());

            return new StatisticService(paperlessBroker, loggingBroker);
        }

        [TestMethod]
        public async Task ShouldRetrieveStatisticAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IStatisticService statisticService = CreateStatisticService();

            // when
            Statistic statistic = await statisticService.RetrieveStatisticAsync();

            // then
            statistic.DocumentsTotal.Should().BeGreaterThanOrEqualTo(0);

            // Ohne Posteingangs-Tag liefert Paperless hier null statt 0.
            if (statistic.DocumentsInbox is int documentsInbox)
            {
                documentsInbox.Should().BeInRange(0, statistic.DocumentsTotal);
            }

            // Jede gemeldete Dateiart belegt die snake_case-Zuordnung der Liste.
            statistic.DocumentFileTypeCounts.Should().OnlyContain(fileTypeCount =>
                !string.IsNullOrWhiteSpace(fileTypeCount.MimeType)
                && fileTypeCount.MimeTypeCount > 0);
        }
    }
}
