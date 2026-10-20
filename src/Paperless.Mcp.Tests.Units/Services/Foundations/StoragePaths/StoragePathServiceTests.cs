using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Services.Foundations.StoragePaths;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    [TestClass]
    public partial class StoragePathServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IStoragePathService storagePathService;

        public StoragePathServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.storagePathService = new StoragePathService(
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

        private static StoragePath CreateRandomStoragePath() =>
            CreateStoragePathFiller().Create();

        private static StoragePathPage CreateRandomStoragePathPage()
        {
            var filler = new Filler<StoragePathPage>();

            filler.Setup()
                .OnProperty(storagePathPage => storagePathPage.Count).Use(GetRandomNumber)
                .OnProperty(storagePathPage => storagePathPage.Results).Use(
                    CreateStoragePathFiller().Create(count: 3).ToList());

            return filler.Create();
        }

        private static Filler<StoragePath> CreateStoragePathFiller()
        {
            var filler = new Filler<StoragePath>();

            filler.Setup()
                .OnProperty(storagePath => storagePath.Id).Use(GetRandomNumber)
                .OnProperty(storagePath => storagePath.Name).Use(GetRandomName)
                .OnProperty(storagePath => storagePath.Path).Use(GetRandomName)
                .OnProperty(storagePath => storagePath.MatchingAlgorithm)
                    .Use(StoragePathMatchingAlgorithm.Any)
                .OnProperty(storagePath => storagePath.Match).Use(GetRandomName);

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
