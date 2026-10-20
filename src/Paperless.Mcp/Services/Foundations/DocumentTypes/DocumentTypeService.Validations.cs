using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;

namespace Paperless.Mcp.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeService
    {
        private const int NameMaximumLength = 128;

        // Reihenfolge ist Pflicht: erst strukturell, dann logisch.
        private static void ValidateDocumentTypeOnAdd(DocumentType documentType)
        {
            ValidateDocumentTypeIsNotNull(documentType);

            Validate(
                (Rule: IsInvalid(documentType.Name), Parameter: nameof(DocumentType.Name)),
                (Rule: ExceedsMaximumLength(documentType.Name), Parameter: nameof(DocumentType.Name)),
                (Rule: IsUndefined(documentType.MatchingAlgorithm), Parameter: nameof(DocumentType.MatchingAlgorithm)));
        }

        private static void ValidateDocumentTypeOnModify(DocumentType documentType)
        {
            ValidateDocumentTypeIsNotNull(documentType);

            Validate(
                (Rule: IsInvalid(documentType.Id), Parameter: nameof(DocumentType.Id)),
                (Rule: IsInvalid(documentType.Name), Parameter: nameof(DocumentType.Name)),
                (Rule: ExceedsMaximumLength(documentType.Name), Parameter: nameof(DocumentType.Name)),
                (Rule: IsUndefined(documentType.MatchingAlgorithm), Parameter: nameof(DocumentType.MatchingAlgorithm)));
        }

        private static void ValidateDocumentTypeId(int documentTypeId) =>
            Validate((Rule: IsInvalid(documentTypeId), Parameter: nameof(DocumentType.Id)));

        private static void ValidatePageRequest(int page, int pageSize) =>
            Validate(
                (Rule: IsInvalid(page), Parameter: "Page"),
                (Rule: IsInvalid(pageSize), Parameter: "PageSize"));

        private static void ValidateDocumentTypeIsNotNull(DocumentType documentType)
        {
            if (documentType is null)
            {
                throw new NullDocumentTypeException(message: "DocumentType is null.");
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

        private static dynamic IsUndefined(DocumentTypeMatchingAlgorithm matchingAlgorithm) => new
        {
            Condition = Enum.IsDefined(matchingAlgorithm) is false,
            Message = "Value is not a defined matching algorithm",
        };

        // Continuous validation: alle verletzten Regeln sammeln, dann den Stromkreis
        // unterbrechen -- der Aufrufer sieht jeden Fehler auf einmal.
        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentTypeException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentTypeException.ThrowIfContainsErrors();
        }
    }
}
