using Paperless.Mcp.Models.Orchestrations.Documents.Exceptions;

namespace Paperless.Mcp.Services.Orchestrations.Documents
{
    public partial class DocumentOrchestrationService
    {
        // Obergrenze fuer die Wartezeit. Ein Werkzeugaufruf, der minutenlang haengt,
        // blockiert den Assistenten; laenger zu warten ist ausserdem unnoetig, weil der
        // Auftrag auch ohne uns weiterlaeuft und sich nachschlagen laesst.
        public const int MaxTimeoutSeconds = 120;

        public const int DefaultTimeoutSeconds = 30;

        private static void ValidateTimeout(int timeoutSeconds) =>
            Validate((Rule: IsInvalidTimeout(timeoutSeconds), Parameter: "TimeoutSeconds"));

        private static dynamic IsInvalidTimeout(int timeoutSeconds) => new
        {
            Condition = timeoutSeconds <= 0 || timeoutSeconds > MaxTimeoutSeconds,
            Message = $"Value must be between 1 and {MaxTimeoutSeconds}",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentOrchestrationException =
                new InvalidDocumentOrchestrationException(
                    message: "Document orchestration is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentOrchestrationException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentOrchestrationException.ThrowIfContainsErrors();
        }
    }
}
