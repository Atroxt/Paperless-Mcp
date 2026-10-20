using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Services.Foundations.DocumentTypes;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    [TestClass]
    public partial class DocumentTypeServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IDocumentTypeService documentTypeService;

        public DocumentTypeServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.documentTypeService = new DocumentTypeService(
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

        private static DocumentType CreateRandomDocumentType() =>
            CreateDocumentTypeFiller().Create();

        private static DocumentTypePage CreateRandomDocumentTypePage()
        {
            var filler = new Filler<DocumentTypePage>();

            filler.Setup()
                .OnProperty(documentTypePage => documentTypePage.Count).Use(GetRandomNumber)
                .OnProperty(documentTypePage => documentTypePage.Results).Use(
                    CreateDocumentTypeFiller().Create(count: 3).ToList());

            return filler.Create();
        }

        private static Filler<DocumentType> CreateDocumentTypeFiller()
        {
            var filler = new Filler<DocumentType>();

            filler.Setup()
                .OnProperty(documentType => documentType.Id).Use(GetRandomNumber)
                .OnProperty(documentType => documentType.Name).Use(GetRandomName)
                .OnProperty(documentType => documentType.MatchingAlgorithm)
                    .Use(DocumentTypeMatchingAlgorithm.Any)
                .OnProperty(documentType => documentType.Match).Use(GetRandomName);

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
