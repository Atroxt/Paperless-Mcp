using Paperless.Mcp.Models.Foundations.Tasks;
using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;

namespace Paperless.Mcp.Services.Foundations.Tasks
{
    public partial class TaskService
    {
        private static void ValidateTaskId(string taskId) =>
            Validate((Rule: IsInvalid(taskId), Parameter: nameof(PaperlessTask.TaskId)));

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidPaperlessTaskException =
                new InvalidPaperlessTaskException(
                    message: "PaperlessTask is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidPaperlessTaskException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidPaperlessTaskException.ThrowIfContainsErrors();
        }
    }
}
