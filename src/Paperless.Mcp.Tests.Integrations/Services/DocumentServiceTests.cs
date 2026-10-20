using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Rein lesend. Diese Tests legen kein Dokument an -- das Hochladen kommt erst mit
    // Slice 8. Sie brauchen deshalb mindestens ein Dokument in der Testinstanz und
    // melden sich sonst als inconclusive ab.
    [TestClass]
    public class DocumentServiceTests
    {
        private static IDocumentService CreateDocumentService()
        {
            IPaperlessBroker paperlessBroker = IntegrationTestConfiguration.CreateBroker();

            ILoggingBroker loggingBroker =
                new LoggingBroker(new NullLogger<LoggingBroker>());

            return new DocumentService(paperlessBroker, loggingBroker);
        }

        private static async Task<Document> RequireAnyDocumentAsync(
            IDocumentService documentService)
        {
            DocumentPage page = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch { Page = 1, PageSize = 1 });

            if (page.Results.Count is 0)
            {
                Assert.Inconclusive(
                    "Uebersprungen: die Testinstanz enthaelt kein Dokument. "
                    + "Mindestens eines hochladen, dann laufen diese Tests.");
            }

            return page.Results[0];
        }

        [TestMethod]
        public async Task ShouldRetrieveDocumentPageAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();

            // when
            DocumentPage actualPage = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch { Page = 1, PageSize = 10 });

            // then
            actualPage.Should().NotBeNull();
            actualPage.Count.Should().BeGreaterThanOrEqualTo(0);
            actualPage.Results.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ShouldRetrieveDocumentByIdAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();
            Document anyDocument = await RequireAnyDocumentAsync(documentService);

            // when
            Document actualDocument =
                await documentService.RetrieveDocumentByIdAsync(anyDocument.Id);

            // then
            actualDocument.Id.Should().Be(anyDocument.Id);
            actualDocument.Title.Should().NotBeNullOrWhiteSpace();

            // Belegt die snake_case-Zuordnung der mehrteiligen Feldnamen
            actualDocument.OriginalFileName.Should().NotBeNullOrWhiteSpace();
            actualDocument.MimeType.Should().NotBeNullOrWhiteSpace();

            // created liefert Paperless als reines Datum, added mit Uhrzeit
            actualDocument.Created.Should().NotBeNull();
            actualDocument.Added.Should().NotBeNull();
        }

        [TestMethod]
        public async Task ShouldFindDocumentByFullTextQueryAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();
            Document anyDocument = await RequireAnyDocumentAsync(documentService);

            // Ein Wort aus dem Titel als Suchbegriff
            string searchTerm = anyDocument.Title.Split('-', ' ')
                .FirstOrDefault(part => part.Length > 3) ?? anyDocument.Title;

            // when
            DocumentPage actualPage = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch { TitleContains = searchTerm, Page = 1, PageSize = 10 });

            // then
            actualPage.Results.Should().Contain(document => document.Id == anyDocument.Id);
        }

        [TestMethod]
        public async Task ShouldFindNothingForImpossibleQueryAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();

            // when
            DocumentPage actualPage = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch
                {
                    Query = $"zzz-gibt-es-nicht-{Guid.NewGuid():N}",
                    Page = 1,
                    PageSize = 10,
                });

            // then
            actualPage.Count.Should().Be(0);
            actualPage.Results.Should().BeEmpty();
        }

        [TestMethod]
        public async Task ShouldReadCustomFieldValuesAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();
            Document anyDocument = await RequireAnyDocumentAsync(documentService);

            // when
            Document actualDocument =
                await documentService.RetrieveDocumentByIdAsync(anyDocument.Id);

            // then
            // Die Liste darf leer sein, aber jeder vorhandene Eintrag muss die Form
            // {field, value} tragen -- das belegt die Zuordnung des verschachtelten
            // Modells samt dem typoffenen Wert.
            actualDocument.CustomFields.Should().NotBeNull();

            actualDocument.CustomFields.Should().AllSatisfy(customFieldValue =>
                customFieldValue.Field.Should().BeGreaterThan(0));
        }

        [TestMethod]
        public async Task ShouldSortDescendingByCreatedAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();

            // when
            DocumentPage actualPage = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch { Ordering = "-created", Page = 1, PageSize = 50 });

            // then
            actualPage.Results
                .Select(document => document.Created)
                .Should().BeInDescendingOrder();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionIfDocumentDoesNotExistAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();
            const int unlikelyDocumentId = 999_999_999;

            // when
            Func<Task<Document>> retrieveTask = async () =>
                await documentService.RetrieveDocumentByIdAsync(unlikelyDocumentId);

            // then
            await retrieveTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>();
        }
    }
}
