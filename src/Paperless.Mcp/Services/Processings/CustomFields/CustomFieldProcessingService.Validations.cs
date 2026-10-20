using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Processings.CustomFields.Exceptions;

namespace Paperless.Mcp.Services.Processings.CustomFields
{
    public partial class CustomFieldProcessingService
    {
        // Processing Services validieren nur, was sie selbst verwenden. Die Id prueft
        // zwar auch die Foundation Service, aber hier wird mit ihr vorher schon
        // gearbeitet.
        private static void ValidateCustomFieldIdOnRemove(int customFieldId) =>
            Validate((Rule: IsInvalid(customFieldId), Parameter: nameof(CustomField.Id)));

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidCustomFieldProcessingException =
                new InvalidCustomFieldProcessingException(
                    message: "CustomField processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidCustomFieldProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidCustomFieldProcessingException.ThrowIfContainsErrors();
        }
    }
}
