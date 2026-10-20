using Paperless.Mcp.Models.Foundations.Tasks;

namespace Paperless.Mcp.Services.Foundations.Tasks
{
    public interface ITaskService
    {
        ValueTask<PaperlessTask> RetrieveTaskByTaskIdAsync(string taskId);
    }
}
