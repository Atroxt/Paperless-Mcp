using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Processings.Correspondents.Exceptions;

namespace Paperless.Mcp.Services.Processings.Correspondents
{
    public partial class CorrespondentProcessingService
    {
        // Processing Services validieren nur, was sie selbst verwenden. Die Id prueft
        // zwar auch die Foundation Service, aber hier wird mit ihr vorher schon
        // gearbeitet.
        private static void ValidateCorrespondentIdOnRemove(int correspondentId) =>
            Validate((Rule: IsInvalid(correspondentId), Parameter: nameof(Correspondent.Id)));

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidCorrespondentProcessingException =
                new InvalidCorrespondentProcessingException(
                    message: "Correspondent processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidCorrespondentProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidCorrespondentProcessingException.ThrowIfContainsErrors();
        }
    }
}
