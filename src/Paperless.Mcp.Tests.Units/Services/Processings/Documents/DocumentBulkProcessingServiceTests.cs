using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Processings.Documents;
using Tynamix.ObjectFiller;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Documents
{
    [TestClass]
    public partial class DocumentBulkProcessingServiceTests
    {
        private readonly Mock<IDocumentService> documentServiceMock;
        private readonly IDocumentBulkProcessingService documentBulkProcessingService;

        public DocumentBulkProcessingServiceTests()
        {
            this.documentServiceMock = new Mock<IDocumentService>();

            this.documentBulkProcessingService =
                new DocumentBulkProcessingService(this.documentServiceMock.Object);
        }

        // Stellt die Leseabfrage so, dass sie genau die genannten Dokumente findet.
        private void SetUpFoundDocuments(params Document[] documents)
        {
            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ReturnsAsync(new DocumentPage
                    {
                        Count = documents.Length,
                        Results = [.. documents],
                    });
        }

        private static Document CreateDocument(int id) =>
            new() { Id = id, Title = GetRandomName() };

        private static string GetRandomName() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();
    }
}
