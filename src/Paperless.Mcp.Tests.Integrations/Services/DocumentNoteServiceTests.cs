using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Foundations.Notes.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Notes;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Prueft gegen eine echte Instanz, was Unit-Tests nicht koennen: ob die JSON-Form
    // stimmt. Insbesondere, ob Paperless auf POST und DELETE tatsaechlich die
    // vollstaendige Notizliste zurueckgibt und ob das "user"-Feld in der Form ankommt,
    // die der DocumentNoteJsonConverter erwartet.
    [TestClass]
    public class DocumentNoteServiceTests
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

        // Eindeutig als Testdatum erkennbar, damit versehentlich zurueckgebliebene
        // Notizen in der Instanz sofort zuzuordnen sind.
        private static string CreateTestNoteText() =>
            $"zz-integrationstest-{Guid.NewGuid():N}";

        [TestMethod]
        public async Task ShouldWalkThroughDocumentNoteLifecycleAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = CreateLoggingBroker();
            var documentService = new DocumentService(broker, loggingBroker);
            var documentNoteService = new DocumentNoteService(broker, loggingBroker);

            Document document = await RequireAnyDocumentAsync(documentService);
            string noteText = CreateTestNoteText();
            int addedNoteId = 0;

            try
            {
                // when: anlegen
                List<DocumentNote> afterAdd =
                    await documentNoteService.AddDocumentNoteAsync(document.Id, noteText);

                // then
                afterAdd.Should().Contain(documentNote => documentNote.Note == noteText);

                DocumentNote addedNote =
                    afterAdd.Single(documentNote => documentNote.Note == noteText);

                addedNote.Id.Should().BeGreaterThan(0);
                addedNote.Document.Should().Be(document.Id);

                // Belegt, dass der Converter das Datum liest statt es auf default zu lassen
                addedNote.Created.Should().NotBe(default);

                addedNoteId = addedNote.Id;

                // when: lesen
                List<DocumentNote> retrievedNotes =
                    await documentNoteService.RetrieveDocumentNotesAsync(document.Id);

                // then
                retrievedNotes.Should().Contain(documentNote => documentNote.Id == addedNoteId);

                retrievedNotes.Should().OnlyContain(documentNote =>
                    documentNote.Document == document.Id);

                // when: loeschen
                List<DocumentNote> afterRemove =
                    await documentNoteService.RemoveDocumentNoteAsync(document.Id, addedNoteId);

                addedNoteId = 0;

                // then
                afterRemove.Should().NotContain(documentNote => documentNote.Note == noteText);
            }
            finally
            {
                if (addedNoteId is not 0)
                {
                    await IntegrationTestConfiguration.CleanUpAsync(
                        $"Notiz {addedNoteId} an Dokument {document.Id}",
                        async () => await documentNoteService.RemoveDocumentNoteAsync(
                            document.Id,
                            addedNoteId));
                }
            }
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfDocumentDoesNotExistAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            var documentNoteService = new DocumentNoteService(broker, CreateLoggingBroker());
            const int unlikelyDocumentId = 999_999_999;

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await documentNoteService.RetrieveDocumentNotesAsync(unlikelyDocumentId);

            // then
            // Belegt die Zuordnung aus der Tabelle in README.md: eine echte 404 auf
            // eine Ressource ist Abhaengigkeitsvalidierung, nicht kritisch.
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteDependencyValidationException>();
        }
    }
}
