using System.Linq.Expressions;
using Moq;
using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.UploadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks;
using Paperless.Mcp.Services.Foundations.UploadLinks;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.UploadLinks
{
    [TestClass]
    public partial class UploadLinkServiceTests
    {
        private const int LifetimeSeconds = 900;

        private readonly Mock<IUploadLinkBroker> uploadLinkBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IUploadLinkService uploadLinkService;

        public UploadLinkServiceTests()
        {
            this.uploadLinkBrokerMock = new Mock<IUploadLinkBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.uploadLinkService = new UploadLinkService(
                uploadLinkBroker: this.uploadLinkBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object,
                configurations: new UploadLinkConfigurations
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

        private static UploadLink CreateRandomUploadLink(DateTimeOffset expiresAt) =>
            new UploadLink
            {
                Id = GetRandomId(),
                FileName = $"{GetRandomText()}.pdf",
                Title = GetRandomText(),
                CorrespondentId = GetRandomNumber(),
                DocumentTypeId = GetRandomNumber(),
                StoragePathId = GetRandomNumber(),
                Created = new DateOnly(2024, 5, 17),
                Token = GetRandomText(),
                ExpiresAt = expiresAt,
            };

        private static string GetRandomId() =>
            Guid.NewGuid().ToString("N");

        private static string GetRandomText() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new DateTimeRange(earliestDate: new DateTime(2024, 1, 1)).GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
