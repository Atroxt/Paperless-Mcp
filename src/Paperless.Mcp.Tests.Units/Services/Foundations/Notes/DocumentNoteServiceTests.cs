using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Services.Foundations.Notes;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Notes
{
    [TestClass]
    public partial class DocumentNoteServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IDocumentNoteService documentNoteService;

        public DocumentNoteServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.documentNoteService = new DocumentNoteService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        // Kritische Abhaengigkeitsfehler: die API ist nicht erreichbar oder nicht
        // zugaenglich, also ein Konfigurations- oder Infrastrukturproblem.
        // HttpResponseNotFoundException gehoert bewusst NICHT dazu -- eine 404 auf eine
        // Ressource heisst "dieses Dokument oder diese Notiz gibt es nicht" und wird als
        // Abhaengigkeitsvalidierung behandelt.
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
            yield return [string.Empty];
            yield return ["   "];
        }

        public static IEnumerable<object[]> InvalidNumbers()
        {
            yield return [0];
            yield return [-1];
            yield return [int.MinValue];
        }

        private static List<DocumentNote> CreateRandomDocumentNotes(int documentId) =>
            CreateDocumentNoteFiller(documentId).Create(count: 3).ToList();

        private static Filler<DocumentNote> CreateDocumentNoteFiller(int documentId)
        {
            var filler = new Filler<DocumentNote>();

            filler.Setup()
                .OnProperty(documentNote => documentNote.Id).Use(GetRandomNumber)
                .OnProperty(documentNote => documentNote.Note).Use(GetRandomText)

                // ObjectFiller kennt DateTimeOffset nicht und wirft sonst beim Fuellen.
                .OnProperty(documentNote => documentNote.Created).Use(GetRandomDateTimeOffset)
                .OnProperty(documentNote => documentNote.User).Use(() => (int?)GetRandomNumber())
                .OnProperty(documentNote => documentNote.Document).Use(documentId);

            return filler;
        }

        private static DateTimeOffset GetRandomDateTimeOffset() =>
            new(new DateTime(2024, 1, 1).AddMinutes(GetRandomNumber()), TimeSpan.Zero);

        private static string GetRandomText() =>
            new MnemonicString(wordCount: 5).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();

        // Der LoggingBroker nimmt Exception entgegen, deshalb der Cast: Pattern-Matching
        // ist in Ausdrucksbaeumen nicht erlaubt, ein Cast schon.
        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
