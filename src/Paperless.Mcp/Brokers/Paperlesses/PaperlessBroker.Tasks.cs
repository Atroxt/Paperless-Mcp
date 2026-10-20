using Paperless.Mcp.Models.Foundations.Tasks;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string TasksRelativeUrl = "api/tasks/";

        // Der Endpunkt ist paginiert, auch bei einer Filterung auf genau eine Kennung.
        public async ValueTask<PaperlessTaskPage> GetTaskPageByTaskIdAsync(string taskId) =>
            await GetAsync<PaperlessTaskPage>(
                $"{TasksRelativeUrl}?task_id={Uri.EscapeDataString(taskId)}");
    }
}
