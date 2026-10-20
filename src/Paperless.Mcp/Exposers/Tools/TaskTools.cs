using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Tasks;
using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;
using Paperless.Mcp.Services.Foundations.Tasks;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    [McpServerToolType]
    public class TaskTools
    {
        private readonly ITaskService taskService;

        public TaskTools(ITaskService taskService) =>
            this.taskService = taskService;

        [McpServerTool(Name = "paperless_tasks_get")]
        [Description(
            "Looks up a background task of Paperless-ngx by its id, for example the one "
            + "returned by paperless_documents_upload. Status is lower case, for example "
            + "pending, started, success or failure; relatedDocumentIds names the "
            + "documents it produced.")]
    public async ValueTask<PaperlessTask> GetTaskAsync(
            [Description("Task id as returned by the upload.")] string taskId)
        {
            try
            {
                return await this.taskService.RetrieveTaskByTaskIdAsync(taskId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                PaperlessTaskValidationException
                    or PaperlessTaskDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The Paperless server could not be reached or failed to answer. "
                    + "Check the server and the connection settings."),
            };

        private static string DescribeInvalidInput(Xeption exception)
        {
            Exception? inner = exception.InnerException;
            string reason = inner?.Message ?? exception.Message;

            if (inner is null || inner.Data.Count is 0)
            {
                return reason;
            }

            IEnumerable<string> details = inner.Data.Keys
                .Cast<object>()
                .Select(key => $"{key}: {Describe(inner.Data[key])}");

            return $"{reason} {string.Join("; ", details)}";
        }

        private static string Describe(object? value) =>
            value switch
            {
                IEnumerable<string> messages => string.Join(", ", messages),
                null => string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
    }
}
