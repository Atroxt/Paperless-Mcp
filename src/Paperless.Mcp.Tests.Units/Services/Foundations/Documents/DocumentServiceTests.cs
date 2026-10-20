using System.Linq.Expressions;
using System.Net.Http;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Documents;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    [TestClass]
    public partial class DocumentServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IDocumentService documentService;

        public DocumentServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.documentService = new DocumentService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        public static IEnumerable<object[]> CriticalDependencyExceptions()
        {
            yield return [new HttpResponseUrlNotFoundException()];
        }

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

        public static IEnumerable<object[]> InvalidNumbers()
        {
            yield return [0];
            yield return [-1];
            yield return [int.MinValue];
        }

        // Von Hand statt mit ObjectFiller: Document traegt DateOnly?, DateTimeOffset? und
        // ein object?, fuer die der Filler keinen Randomizer kennt.
        private static Document CreateRandomDocument() =>
            new()
            {
                Id = GetRandomNumber(),
                Title = GetRandomName(),
                Content = GetRandomName(),
                Correspondent = GetRandomNumber(),
                DocumentType = GetRandomNumber(),
                StoragePath = GetRandomNumber(),
                Tags = [GetRandomNumber(), GetRandomNumber()],
                Created = new DateOnly(2026, 1, 15),
                Added = new DateTimeOffset(2026, 1, 15, 9, 0, 0, TimeSpan.Zero),
                Modified = new DateTimeOffset(2026, 1, 16, 9, 0, 0, TimeSpan.Zero),
                ArchiveSerialNumber = GetRandomNumber(),
                OriginalFileName = $"{GetRandomName()}.pdf",
                MimeType = "application/pdf",
                PageCount = GetRandomNumber(),
                Owner = GetRandomNumber(),
                UserCanChange = true,
                CustomFields =
                [
                    new DocumentCustomFieldValue { Field = GetRandomNumber(), Value = "EUR1.00" },
                ],
            };

        private static DocumentPage CreateRandomDocumentPage() =>
            new()
            {
                Count = GetRandomNumber(),
                Results = [CreateRandomDocument(), CreateRandomDocument()],
            };

        private static DocumentSearch CreateRandomDocumentSearch() =>
            new()
            {
                Query = GetRandomName(),
                Page = GetRandomNumber(),
                PageSize = GetRandomNumber(),
            };

        private static string GetRandomName() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
