using System.Linq.Expressions;
using Moq;
using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.DownloadLinks;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Services.Foundations.DownloadLinks;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DownloadLinks
{
    [TestClass]
    public partial class DownloadLinkServiceTests
    {
        // Die Lebensdauer ist in den Tests fest, nicht zufaellig: sie steht in jeder
        // Erwartung an ExpiresAt, und ein Zufallswert wuerde den Rechenfehler, den
        // dieser Test finden soll, mitrechnen.
        private const int LifetimeSeconds = 300;

        private readonly Mock<IDownloadLinkBroker> downloadLinkBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IDownloadLinkService downloadLinkService;

        public DownloadLinkServiceTests()
        {
            this.downloadLinkBrokerMock = new Mock<IDownloadLinkBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.downloadLinkService = new DownloadLinkService(
                downloadLinkBroker: this.downloadLinkBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                configurations: new DownloadLinkConfigurations
                {
                    LifetimeSeconds = LifetimeSeconds,
                });
        }

        public static IEnumerable<object[]> InvalidTexts()
        {
            yield return [null!];
            yield return [string.Empty];
            yield return ["   "];
        }

        public static IEnumerable<object[]> InvalidNumbers()
        {
            yield return [0];
            yield return [-1];
            yield return [int.MinValue];
        }

        private static DownloadLink CreateRandomDownloadLink(DateTimeOffset expiresAt) =>
            new DownloadLink
            {
                Id = GetRandomId(),
                DocumentId = GetRandomNumber(),
                Original = false,
                Token = GetRandomToken(),
                ExpiresAt = expiresAt,
            };

        private static string GetRandomId() =>
            Guid.NewGuid().ToString("N");

        private static string GetRandomToken() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime(2024, 1, 1)).GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
