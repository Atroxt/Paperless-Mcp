using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.Delays;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Tasks;
using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;
using Paperless.Mcp.Models.Orchestrations.Documents;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Tasks;

namespace Paperless.Mcp.Services.Orchestrations.Documents
{
    // Die erste Orchestration Service des Projekts: sie fuehrt zwei Entities zusammen,
    // Dokumente und Auftraege. Genau das unterscheidet sie von einer Processing Service,
    // die immer nur eine Entity kennt.
    //
    // Der Ablauf ist der Grund fuer ihre Existenz. Ein Upload beantwortet Paperless mit
    // einer Auftragskennung, nicht mit einem Dokument; wer das Dokument will, muss den
    // Auftrag verfolgen und danach lesen. Ohne diese Schicht muesste der Assistent drei
    // Werkzeuge in der richtigen Reihenfolge aufrufen und dazwischen selbst warten.
    public partial class DocumentOrchestrationService : IDocumentOrchestrationService
    {
        // Eine Sekunde: Paperless braucht fuer eine kleine Datei unter einer Sekunde,
        // und kuerzer zu fragen belastet die Instanz, ohne frueher fertig zu sein.
        private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

        private readonly IDocumentService documentService;
        private readonly ITaskService taskService;
        private readonly IDelayBroker delayBroker;
        private readonly IDateTimeBroker dateTimeBroker;
        private readonly ILoggingBroker loggingBroker;

        public DocumentOrchestrationService(
            IDocumentService documentService,
            ITaskService taskService,
            IDelayBroker delayBroker,
            IDateTimeBroker dateTimeBroker,
            ILoggingBroker loggingBroker)
        {
            this.documentService = documentService;
            this.taskService = taskService;
            this.delayBroker = delayBroker;
            this.dateTimeBroker = dateTimeBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<DocumentUploadResult> AddDocumentAndAwaitAsync(
            DocumentUpload documentUpload,
            int timeoutSeconds) =>
            TryCatch(async () =>
            {
                ValidateTimeout(timeoutSeconds);

                // Das Hochladen selbst validiert die Foundation Service.
                string taskId = await this.documentService.AddDocumentFileAsync(documentUpload);

                DateTimeOffset deadline =
                    (await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync())
                        .AddSeconds(timeoutSeconds);

                string lastStatus = UnknownStatus;

                while (true)
                {
                    await this.delayBroker.DelayAsync(PollInterval);

                    PaperlessTask? task = await TryRetrieveTaskAsync(taskId);

                    if (task is not null)
                    {
                        lastStatus = task.Status;

                        if (IsFailure(task))
                        {
                            return CreateFailedResult(taskId, task);
                        }

                        if (IsSuccess(task))
                        {
                            return await CreateCompletedResultAsync(taskId, task);
                        }
                    }

                    DateTimeOffset now =
                        await this.dateTimeBroker.GetCurrentDateTimeOffsetAsync();

                    if (now >= deadline)
                    {
                        return CreatePendingResult(taskId, lastStatus, timeoutSeconds);
                    }
                }
            });

        // Direkt nach dem Upload gibt es die Auftragszeile noch nicht -- an der Instanz
        // beobachtet. Waehrend des Wartens ist "nicht gefunden" deshalb keine Auskunft
        // ueber einen Fehler, sondern ueber den Zeitpunkt: einfach noch einmal fragen.
        private async ValueTask<PaperlessTask?> TryRetrieveTaskAsync(string taskId)
        {
            try
            {
                return await this.taskService.RetrieveTaskByTaskIdAsync(taskId);
            }
            catch (PaperlessTaskDependencyValidationException)
            {
                return null;
            }
        }

        private async ValueTask<DocumentUploadResult> CreateCompletedResultAsync(
            string taskId,
            PaperlessTask task)
        {
            int? documentId = task.RelatedDocumentIds.Count > 0
                ? task.RelatedDocumentIds[0]
                : null;

            if (documentId is null)
            {
                return new DocumentUploadResult
                {
                    TaskId = taskId,
                    Status = task.Status,
                    Completed = true,

                    Summary =
                        "Paperless finished the upload but did not name a document. "
                        + $"Use paperless_tasks_get with task id {taskId} for the details.",
                };
            }

            Document document =
                await this.documentService.RetrieveDocumentByIdAsync(documentId.Value);

            return new DocumentUploadResult
            {
                TaskId = taskId,
                Status = task.Status,
                Completed = true,
                DocumentId = document.Id,
                Document = document,
                Summary = $"Document '{document.Title}' was created with id {document.Id}.",
            };
        }

        private static DocumentUploadResult CreateFailedResult(
            string taskId,
            PaperlessTask task) =>
            new()
            {
                TaskId = taskId,
                Status = task.Status,
                Completed = true,

                Summary =
                    "Paperless could not process the file. "
                    + $"Use paperless_tasks_get with task id {taskId} for the details.",
            };

        // Kein Fehler: der Auftrag laeuft weiter, nur ohne uns.
        private static DocumentUploadResult CreatePendingResult(
            string taskId,
            string status,
            int timeoutSeconds) =>
            new()
            {
                TaskId = taskId,
                Status = status,
                Completed = false,

                Summary =
                    $"Paperless is still working after {timeoutSeconds} second(s). "
                    + "The upload is not lost; use paperless_tasks_get with task id "
                    + $"{taskId} to follow it.",
            };

        private static bool IsSuccess(PaperlessTask task) =>
            string.Equals(task.Status, SuccessStatus, StringComparison.OrdinalIgnoreCase);

        // revoked zaehlt dazu: der Auftrag wurde abgebrochen und kommt nicht wieder.
        private static bool IsFailure(PaperlessTask task) =>
            string.Equals(task.Status, FailureStatus, StringComparison.OrdinalIgnoreCase)
            || string.Equals(task.Status, RevokedStatus, StringComparison.OrdinalIgnoreCase);

        // Paperless liefert klein geschriebene Zeichenketten -- an der Instanz geprueft.
        private const string SuccessStatus = "success";

        private const string FailureStatus = "failure";

        private const string RevokedStatus = "revoked";

        private const string UnknownStatus = "unknown";
    }
}
