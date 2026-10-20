using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Services.Foundations.CustomFields;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    [TestClass]
    public partial class CustomFieldServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ICustomFieldService customFieldService;

        public CustomFieldServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.customFieldService = new CustomFieldService(
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

        private static CustomField CreateRandomCustomField() =>
            CreateCustomFieldFiller().Create();

        private static CustomFieldPage CreateRandomCustomFieldPage()
        {
            var filler = new Filler<CustomFieldPage>();

            filler.Setup()
                .OnProperty(customFieldPage => customFieldPage.Count).Use(GetRandomNumber)
                .OnProperty(customFieldPage => customFieldPage.Results).Use(
                    CreateCustomFieldFiller().Create(count: 3).ToList());

            return filler.Create();
        }

        private static Filler<CustomField> CreateCustomFieldFiller()
        {
            var filler = new Filler<CustomField>();

            filler.Setup()
                .OnProperty(customField => customField.Id).Use(GetRandomNumber)
                .OnProperty(customField => customField.Name).Use(GetRandomName)
                .OnProperty(customField => customField.DataType).Use(CustomFieldDataType.Text)
                // ExtraData bleibt null: nur Select- und Monetary-Felder fuellen es,
                // und ObjectFiller kann den verschachtelten Typ nicht erzeugen.
                .OnProperty(customField => customField.ExtraData).IgnoreIt();

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
