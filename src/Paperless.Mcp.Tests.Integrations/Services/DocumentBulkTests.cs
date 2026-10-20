using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Tags;
using Paperless.Mcp.Services.Processings.Documents;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Sammeloperationen gegen die echte Instanz. Der bestaetigte Loeschfall fehlt hier
    // bewusst: er wuerde ein Dokument der Testinstanz vernichten, und die Unit-Tests
    // belegen bereits, dass der Aufruf dann hinausgeht. Was hier zaehlt, ist das
    // Gegenteil -- dass der Trockenlauf nachweislich nichts anfasst.
    [TestClass]
    public class DocumentBulkTests
    {
        private static ILoggingBroker CreateLoggingBroker() =>
            new LoggingBroker(new NullLogger<LoggingBroker>());

        private static async Task<Document> RequireAnyDocumentAsync(
            IDocumentService documentService)
        {
            DocumentPage page = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch { Page = 1, PageSize = 1 });

            if (page.Results.Count is 0)
            {
                Assert.Inconclusive(
                    "Uebersprungen: die Testinstanz enthaelt kein Dokument.");
            }

            return page.Results[0];
        }

        [TestMethod]
        public async Task ShouldAddAndRemoveTagInBulkAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = CreateLoggingBroker();

            var documentService = new DocumentService(broker, loggingBroker);
            var tagService = new TagService(broker, loggingBroker);
            var bulkService = new DocumentBulkProcessingService(documentService);

            Document document = await RequireAnyDocumentAsync(documentService);

            Tag addedTag = await tagService.AddTagAsync(new Tag
            {
                Name = $"zz-integrationstest-{Guid.NewGuid():N}"[..40],
            });

            try
            {
                // when
                DocumentBulkPlan dryRun = await bulkService.BulkUpdateDocumentsAsync(
                    new DocumentBulkUpdate
                    {
                        DocumentIds = [document.Id],
                        AddTagIds = [addedTag.Id],
                    },
                    confirm: false);

                // then
                dryRun.Applied.Should().BeFalse();
                dryRun.AffectedDocumentIds.Should().BeEquivalentTo([document.Id]);

                // Der eigentliche Beweis des Trockenlaufs: der Tag haengt NICHT dran.
                Document untouched =
                    await documentService.RetrieveDocumentByIdAsync(document.Id);

                untouched.Tags.Should().NotContain(addedTag.Id);

                // when: bestaetigt
                DocumentBulkPlan applied = await bulkService.BulkUpdateDocumentsAsync(
                    new DocumentBulkUpdate
                    {
                        DocumentIds = [document.Id],
                        AddTagIds = [addedTag.Id],
                    },
                    confirm: true);

                // then
                applied.Applied.Should().BeTrue();

                Document tagged =
                    await documentService.RetrieveDocumentByIdAsync(document.Id);

                tagged.Tags.Should().Contain(addedTag.Id);
            }
            finally
            {
                await IntegrationTestConfiguration.CleanUpAsync(
                    $"Tag {addedTag.Id} von Dokument {document.Id} entfernen",
                    () => documentService.ModifyDocumentsTagsAsync(
                        [document.Id], addTagIds: [], removeTagIds: [addedTag.Id]).AsTask());

                await IntegrationTestConfiguration.CleanUpAsync(
                    $"Tag {addedTag.Id}",
                    () => tagService.RemoveTagByIdAsync(addedTag.Id).AsTask());
            }
        }

        // Paperless laesst unbekannte Ids stillschweigend weg und antwortet trotzdem mit
        // 200 OK. Dass der Bericht sie nennt, ist der Unterschied zwischen einem
        // sichtbaren Tippfehler und einer Aenderung an der falschen Menge.
        [TestMethod]
        public async Task ShouldReportUnknownDocumentIdsAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();

            var documentService = new DocumentService(broker, CreateLoggingBroker());
            var bulkService = new DocumentBulkProcessingService(documentService);

            Document document = await RequireAnyDocumentAsync(documentService);
            const int missingDocumentId = 999_999;

            // when
            DocumentBulkPlan actualPlan = await bulkService.BulkRemoveDocumentsAsync(
                [document.Id, missingDocumentId],
                confirm: false);

            // then
            actualPlan.Applied.Should().BeFalse();
            actualPlan.AffectedDocumentIds.Should().BeEquivalentTo([document.Id]);
            actualPlan.UnknownDocumentIds.Should().BeEquivalentTo([missingDocumentId]);
            actualPlan.Summary.Should().Contain(missingDocumentId.ToString());

            // Und das Dokument ist noch da.
            Document stillThere =
                await documentService.RetrieveDocumentByIdAsync(document.Id);

            stillThere.Id.Should().Be(document.Id);
        }

        // Der Fall, den die Einzelaenderung bis heute nicht kann: eine Zuordnung leeren.
        [TestMethod]
        public async Task ShouldClearCorrespondentInBulkAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();

            var documentService = new DocumentService(broker, CreateLoggingBroker());
            var bulkService = new DocumentBulkProcessingService(documentService);

            Document document = await RequireAnyDocumentAsync(documentService);
            int? originalCorrespondentId = document.Correspondent;

            try
            {
                // when
                await bulkService.BulkUpdateDocumentsAsync(
                    new DocumentBulkUpdate
                    {
                        DocumentIds = [document.Id],
                        Correspondent = new DocumentReferenceUpdate { Clear = true },
                    },
                    confirm: true);

                // then
                Document cleared =
                    await documentService.RetrieveDocumentByIdAsync(document.Id);

                cleared.Correspondent.Should().BeNull();
            }
            finally
            {
                if (originalCorrespondentId is not null)
                {
                    await IntegrationTestConfiguration.CleanUpAsync(
                        $"Korrespondent {originalCorrespondentId} wiederherstellen",
                        () => documentService.ModifyDocumentsCorrespondentAsync(
                            [document.Id], originalCorrespondentId).AsTask());
                }
            }
        }
    }
}
