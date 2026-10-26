using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Processings.Notes;
using Paperless.Mcp.Models.Processings.Notes.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Notes;
using Paperless.Mcp.Services.Processings.Notes;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Der confirm-Guard gegen die echte Instanz. Unit-Tests belegen, dass die Service
    // den Loeschaufruf unterlaesst; hier wird belegt, dass die Notiz danach wirklich
    // noch am Dokument haengt.
    [TestClass]
    public class DocumentNoteRemovalGuardTests
    {
        private static (IDocumentService Documents,
            IDocumentNoteService Foundation,
            IDocumentNoteProcessingService Processing) CreateServices()
        {
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = new LoggingBroker(new NullLogger<LoggingBroker>());

            var documentNoteService = new DocumentNoteService(broker, loggingBroker);

            return (
                new DocumentService(broker, loggingBroker),
                documentNoteService,
                new DocumentNoteProcessingService(documentNoteService));
        }

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

        private static async Task<DocumentNote> AddTestNoteAsync(
            IDocumentNoteService documentNoteService,
            int documentId)
        {
            string noteText = $"zz-integrationstest-{Guid.NewGuid():N}";

            List<DocumentNote> afterAdd =
                await documentNoteService.AddDocumentNoteAsync(documentId, noteText);

            return afterAdd.Single(documentNote => documentNote.Note == noteText);
        }

        [TestMethod]
        public async Task ShouldLeaveDocumentNoteInPlaceOnDryRunAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();

            (IDocumentService documents,
                IDocumentNoteService foundation,
                IDocumentNoteProcessingService processing) = CreateServices();

            Document document = await RequireAnyDocumentAsync(documents);
            DocumentNote addedNote = await AddTestNoteAsync(foundation, document.Id);
            int addedNoteId = addedNote.Id;

            try
            {
                // when
                DocumentNoteRemovalPlan actualPlan =
                    await processing.RemoveDocumentNoteAsync(
                        document.Id,
                        addedNoteId,
                        confirm: false);

                // then
                actualPlan.Removed.Should().BeFalse();
                actualPlan.NoteId.Should().Be(addedNoteId);
                actualPlan.Note.Should().Be(addedNote.Note);
                actualPlan.Summary.Should().Contain("Dry run");

                // Der eigentliche Beweis: die Notiz ist noch da
                List<DocumentNote> stillThere =
                    await foundation.RetrieveDocumentNotesAsync(document.Id);

                stillThere.Should().Contain(documentNote => documentNote.Id == addedNoteId);
            }
            finally
            {
                await IntegrationTestConfiguration.CleanUpAsync(
                    $"Notiz {addedNoteId} an Dokument {document.Id}",
                    () => foundation
                        .RemoveDocumentNoteAsync(document.Id, addedNoteId)
                        .AsTask());
            }
        }

        [TestMethod]
        public async Task ShouldRemoveDocumentNoteWhenConfirmedAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();

            (IDocumentService documents,
                IDocumentNoteService foundation,
                IDocumentNoteProcessingService processing) = CreateServices();

            Document document = await RequireAnyDocumentAsync(documents);
            DocumentNote addedNote = await AddTestNoteAsync(foundation, document.Id);

            // when
            DocumentNoteRemovalPlan actualPlan =
                await processing.RemoveDocumentNoteAsync(
                    document.Id,
                    addedNote.Id,
                    confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();
            actualPlan.Summary.Should().NotContain("Dry run");

            List<DocumentNote> remainingNotes =
                await foundation.RetrieveDocumentNotesAsync(document.Id);

            remainingNotes.Should().NotContain(documentNote => documentNote.Id == addedNote.Id);
        }

        // Eine Notiz-Id, die das Dokument nicht traegt, darf nicht stillschweigend als
        // Erfolg durchgehen.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionIfNoteDoesNotBelongToDocumentAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();

            (IDocumentService documents,
                _,
                IDocumentNoteProcessingService processing) = CreateServices();

            Document document = await RequireAnyDocumentAsync(documents);
            const int unlikelyNoteId = 999_999_999;

            // when
            Func<Task<DocumentNoteRemovalPlan>> removeTask = async () =>
                await processing.RemoveDocumentNoteAsync(
                    document.Id,
                    unlikelyNoteId,
                    confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentNoteProcessingValidationException>();
        }
    }
}
