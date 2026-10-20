using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Tasks;
using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;

namespace Paperless.Mcp.Services.Foundations.Tasks
{
    public partial class TaskService : ITaskService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public TaskService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<PaperlessTask> RetrieveTaskByTaskIdAsync(string taskId) =>
            TryCatch(async () =>
            {
                ValidateTaskId(taskId);

                PaperlessTaskPage taskPage =
                    await this.paperlessBroker.GetTaskPageByTaskIdAsync(taskId);

                // Eine leere Seite ist kein Fehler der Gegenstelle, sondern heisst:
                // diese Kennung kennt Paperless nicht.
                return taskPage.Results.Count > 0
                    ? taskPage.Results[0]
                    : throw new NotFoundPaperlessTaskException(
                        message: "PaperlessTask not found.");
            });
    }
}
