using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Services.Foundations.Correspondents;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    [TestClass]
    public partial class CorrespondentServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ICorrespondentService correspondentService;

        public CorrespondentServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.correspondentService = new CorrespondentService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        // HttpResponseNotFoundException gehoert bewusst nicht dazu: eine 404 auf eine
        // Ressource heisst "diesen Korrespondenten gibt es nicht" und wird als
        // Abhaengigkeitsvalidierung behandelt. Der falsche Endpunkt kommt als
        // HttpResponseUrlNotFoundException.
        public static IEnumerable<object[]> CriticalDependencyExceptions()
        {
            yield return [new HttpResponseUrlNotFoundException()];
        }

        // Ein abgelehntes Token ist behebbare Eingabe des Aufrufers, kein Ausfall:
        // im HTTP-Transport bringt jeder sein eigenes Token mit.
        public static IEnumerable<object[]> UnauthorizedExceptions()
        {
            yield return [new HttpResponseUnauthorizedException()];
            yield return [new HttpResponseForbiddenException()];
        }

        public static IEnumerable<object[]> ServerDependencyExceptions()
        {
            yield return [new HttpResponseInternalServerErrorException()];
            yield return [new HttpResponseServiceUnavailableException()];
            yield return [new HttpResponseException()];
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

        private static Correspondent CreateRandomCorrespondent() =>
            CreateCorrespondentFiller().Create();

        private static CorrespondentPage CreateRandomCorrespondentPage()
        {
            var filler = new Filler<CorrespondentPage>();

            filler.Setup()
                .OnProperty(correspondentPage => correspondentPage.Count).Use(GetRandomNumber)
                .OnProperty(correspondentPage => correspondentPage.Results).Use(
                    CreateCorrespondentFiller().Create(count: 3).ToList());

            return filler.Create();
        }

        private static Filler<Correspondent> CreateCorrespondentFiller()
        {
            var filler = new Filler<Correspondent>();

            filler.Setup()
                // ObjectFiller kennt keinen Randomizer fuer DateTimeOffset?, deshalb
                // hier ein fester Wert. LastCorrespondence vergibt ohnehin Paperless.
                .OnType<DateTimeOffset?>().Use(() =>
                    (DateTimeOffset?)new DateTimeOffset(2026, 1, 15, 9, 0, 0, TimeSpan.Zero))
                .OnProperty(correspondent => correspondent.Id).Use(GetRandomNumber)
                .OnProperty(correspondent => correspondent.Name).Use(GetRandomName)
                .OnProperty(correspondent => correspondent.MatchingAlgorithm)
                    .Use(CorrespondentMatchingAlgorithm.Any)
                .OnProperty(correspondent => correspondent.Match).Use(GetRandomName);

            return filler;
        }

        private static string GetRandomName() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
