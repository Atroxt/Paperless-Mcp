using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;

namespace Paperless.Mcp.Services.Foundations.CustomFields
{
    public partial class CustomFieldService
    {
        private const int NameMaximumLength = 128;

        // Reihenfolge ist Pflicht: erst strukturell, dann logisch.
        private static void ValidateCustomFieldOnAdd(CustomField customField)
        {
            ValidateCustomFieldIsNotNull(customField);

            Validate(
                (Rule: IsInvalid(customField.Name), Parameter: nameof(CustomField.Name)),
                (Rule: ExceedsMaximumLength(customField.Name), Parameter: nameof(CustomField.Name)),
                (Rule: IsUndefined(customField.DataType), Parameter: nameof(CustomField.DataType)),
                (Rule: IsMissingSelectOptions(customField), Parameter: nameof(CustomField.ExtraData)));
        }

        private static void ValidateCustomFieldOnModify(CustomField customField)
        {
            ValidateCustomFieldIsNotNull(customField);

            Validate(
                (Rule: IsInvalid(customField.Id), Parameter: nameof(CustomField.Id)),
                (Rule: IsInvalid(customField.Name), Parameter: nameof(CustomField.Name)),
                (Rule: ExceedsMaximumLength(customField.Name), Parameter: nameof(CustomField.Name)),
                (Rule: IsUndefined(customField.DataType), Parameter: nameof(CustomField.DataType)),
                (Rule: IsMissingSelectOptions(customField), Parameter: nameof(CustomField.ExtraData)));
        }

        private static void ValidateCustomFieldId(int customFieldId) =>
            Validate((Rule: IsInvalid(customFieldId), Parameter: nameof(CustomField.Id)));

        private static void ValidatePageRequest(int page, int pageSize) =>
            Validate(
                (Rule: IsInvalid(page), Parameter: "Page"),
                (Rule: IsInvalid(pageSize), Parameter: "PageSize"));

        private static void ValidateCustomFieldIsNotNull(CustomField customField)
        {
            if (customField is null)
            {
                throw new NullCustomFieldException(message: "CustomField is null.");
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required",
        };

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static dynamic ExceedsMaximumLength(string text) => new
        {
            Condition = text?.Length > NameMaximumLength,
            Message = $"Text exceeds the maximum length of {NameMaximumLength} characters",
        };

        private static dynamic IsUndefined(CustomFieldDataType dataType) => new
        {
            Condition = Enum.IsDefined(dataType) is false,
            Message = "Value is not a defined data type",
        };

        // Paperless weist ein Auswahlfeld ohne Optionen mit 400 ab
        // ("extra_data.select_options must be a valid list"), also hier schon abfangen.
        private static dynamic IsMissingSelectOptions(CustomField customField) => new
        {
            Condition = customField.DataType is CustomFieldDataType.Select
                && (customField.ExtraData?.SelectOptions is null
                    || customField.ExtraData.SelectOptions.Count is 0),

            Message = "Select options are required for a select field",
        };

        // Continuous validation: alle verletzten Regeln sammeln, dann den Stromkreis
        // unterbrechen -- der Aufrufer sieht jeden Fehler auf einmal.
        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidCustomFieldException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidCustomFieldException.ThrowIfContainsErrors();
        }
    }
}
