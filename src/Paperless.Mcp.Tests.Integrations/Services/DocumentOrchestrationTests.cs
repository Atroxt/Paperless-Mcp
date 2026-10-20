using System.Text;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.Delays;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Orchestrations.Documents;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Tasks;
using Paperless.Mcp.Services.Orchestrations.Documents;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Der mehrstufige Ablauf gegen die echte Instanz: hochladen, auf den Auftrag warten,
    // das entstandene Dokument lesen. Genau hier zeigt sich, ob die Annahmen ueber die
    // Auftragszeile stimmen -- etwa, dass sie direkt nach dem Upload noch gar nicht da
    // ist und dass related_document_ids erst mit dem Erfolg gefuellt wird.
    [TestClass]
    public class DocumentOrchestrationTests
    {
        [TestMethod]
        public async Task ShouldUploadAndReturnTheNewDocumentAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = new LoggingBroker(new NullLogger<LoggingBroker>());

            var orchestrationService = new DocumentOrchestrationService(
                documentService: new DocumentService(broker, loggingBroker),
                taskService: new TaskService(broker, loggingBroker),
                delayBroker: new DelayBroker(),
                dateTimeBroker: new DateTimeBroker(),
                loggingBroker: loggingBroker);

            string title = $"zz-integrationstest-{Guid.NewGuid():N}"[..40];

            var documentUpload = new DocumentUpload
            {
                Content = Encoding.UTF8.GetBytes(
                    $"Integrationstest {Guid.NewGuid()}\nBetrag: 1,00 EUR\n"),

                FileName = $"{title}.txt",
                Title = title,
            };

            DocumentUploadResult actualResult = null!;

            try
            {
                // when
                actualResult = await orchestrationService.AddDocumentAndAwaitAsync(
                    documentUpload, timeoutSeconds: 90);

                // then
                actualResult.Completed.Should().BeTrue();
                actualResult.Status.Should().Be("success");
                actualResult.DocumentId.Should().NotBeNull();

                // Der Punkt des ganzen Ablaufs: ein Aufruf, und das Dokument liegt vor.
                actualResult.Document.Should().NotBeNull();
                actualResult.Document!.Title.Should().Be(title);
                actualResult.Document.Id.Should().Be(actualResult.DocumentId!.Value);
            }
            finally
            {
                if (actualResult?.DocumentId is not null)
                {
                    await IntegrationTestConfiguration.CleanUpAsync(
                        $"Dokument {actualResult.DocumentId}",
                        () => IntegrationTestConfiguration.DeleteDocumentAsync(
                            actualResult.DocumentId.Value));
                }
            }
        }
    }
}
