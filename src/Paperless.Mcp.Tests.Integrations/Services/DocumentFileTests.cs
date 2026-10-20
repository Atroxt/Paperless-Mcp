using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Documents;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Hochladen laesst sich nur gegen eine echte Instanz belegen: RESTFulSense baut das
    // Multipart-Formular aus Attributen, und ob Paperless es annimmt, zeigt erst der
    // Aufruf.
    [TestClass]
    public class DocumentFileTests
    {
        private static IDocumentService CreateDocumentService()
        {
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();

            return new DocumentService(broker, new LoggingBroker(new NullLogger<LoggingBroker>()));
        }

        [TestMethod]
        public async Task ShouldUploadDocumentAndReportTaskAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();

            string marker = $"zz-integrationstest-{Guid.NewGuid():N}"[..40];

            var documentUpload = new DocumentUpload
            {
                FileName = $"{marker}.txt",
                Content = Encoding.UTF8.GetBytes($"Testinhalt fuer {marker}\n"),
                Title = marker,
            };

            // when
            string actualTaskId = await documentService.AddDocumentFileAsync(documentUpload);

            // then
            // Paperless antwortet mit der Kennung des Verarbeitungsauftrags, nicht mit
            // einem Dokument -- die Verarbeitung laeuft asynchron.
            actualTaskId.Should().NotBeNullOrWhiteSpace();
            Guid.TryParse(actualTaskId, out _).Should().BeTrue();

            // Aufraeumen: warten, bis das Dokument da ist, dann entfernen
            int? createdDocumentId = await WaitForDocumentAsync(documentService, marker);

            createdDocumentId.Should().NotBeNull(
                "das hochgeladene Dokument muss in der Instanz ankommen");

            await IntegrationTestConfiguration.DeleteDocumentAsync(createdDocumentId!.Value);
        }

        [TestMethod]
        public async Task ShouldRetrieveThumbnailAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentService documentService = CreateDocumentService();

            DocumentPage page = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch { Page = 1, PageSize = 1 });

            if (page.Results.Count is 0)
            {
                Assert.Inconclusive("Uebersprungen: die Testinstanz enthaelt kein Dokument.");
            }

            int documentId = page.Results[0].Id;

            // when
            DocumentThumbnail actualThumbnail =
                await documentService.RetrieveDocumentThumbnailAsync(documentId);

            // then
            actualThumbnail.DocumentId.Should().Be(documentId);
            actualThumbnail.Content.Should().NotBeEmpty();
            actualThumbnail.MimeType.Should().Be("image/webp");

            // WebP beginnt mit "RIFF" und traegt "WEBP" an Position 8
            Encoding.ASCII.GetString(actualThumbnail.Content, 0, 4).Should().Be("RIFF");
            Encoding.ASCII.GetString(actualThumbnail.Content, 8, 4).Should().Be("WEBP");
        }

        private static async Task<int?> WaitForDocumentAsync(
            IDocumentService documentService,
            string title)
        {
            for (int attempt = 0; attempt < 40; attempt++)
            {
                DocumentPage page = await documentService.RetrieveDocumentPageAsync(
                    new DocumentSearch { TitleContains = title, Page = 1, PageSize = 1 });

                if (page.Results.Count > 0)
                {
                    return page.Results[0].Id;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            return null;
        }
    }
}
