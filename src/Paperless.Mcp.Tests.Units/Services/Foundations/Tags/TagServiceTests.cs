using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Services.Foundations.Tags;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    [TestClass]
    public partial class TagServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ITagService tagService;

        public TagServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.tagService = new TagService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        // Kritische Abhaengigkeitsfehler: die API ist nicht erreichbar oder nicht
        // zugaenglich, also ein Konfigurations- oder Infrastrukturproblem.
        // HttpResponseNotFoundException gehoert bewusst NICHT dazu -- eine 404 auf eine
        // Ressource heisst "diesen Tag gibt es nicht" und wird als
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

        // Serverseitige Fehler, moeglicherweise voruebergehend. Die generische
        // HttpResponseException deckt jede sonstige Antwort ausserhalb des
        // Erfolgsbereichs ab und wird gleich behandelt.
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

        private static Tag CreateRandomTag() =>
            CreateTagFiller().Create();

        private static TagPage CreateRandomTagPage()
        {
            var filler = new Filler<TagPage>();

            filler.Setup()
                .OnProperty(tagPage => tagPage.Count).Use(GetRandomNumber)
                .OnProperty(tagPage => tagPage.Results).Use(
                    CreateTagFiller().Create(count: 3).ToList());

            return filler.Create();
        }

        private static Filler<Tag> CreateTagFiller()
        {
            var filler = new Filler<Tag>();

            filler.Setup()
                .OnProperty(tag => tag.Id).Use(GetRandomNumber)
                .OnProperty(tag => tag.Name).Use(GetRandomName)
                .OnProperty(tag => tag.Color).Use("#a1b2c3")
                .OnProperty(tag => tag.TextColor).Use("#000000")
                .OnProperty(tag => tag.MatchingAlgorithm).Use(TagMatchingAlgorithm.Any)
                .OnProperty(tag => tag.Match).Use(GetRandomName);

            return filler;
        }

        private static string GetRandomName() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();

        // Der LoggingBroker nimmt Exception entgegen, deshalb der Cast: Pattern-Matching
        // ist in Ausdrucksbaeumen nicht erlaubt, ein Cast schon.
        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
