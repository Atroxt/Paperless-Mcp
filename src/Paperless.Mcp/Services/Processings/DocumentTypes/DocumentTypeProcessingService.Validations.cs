using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions;

namespace Paperless.Mcp.Services.Processings.DocumentTypes
{
    public partial class DocumentTypeProcessingService
    {
        // Processing Services validieren nur, was sie selbst verwenden. Die Id prueft
        // zwar auch die Foundation Service, aber hier wird mit ihr vorher schon
        // gearbeitet.
        private static void ValidateDocumentTypeIdOnRemove(int documentTypeId) =>
            Validate((Rule: IsInvalid(documentTypeId), Parameter: nameof(DocumentType.Id)));

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentTypeProcessingException =
                new InvalidDocumentTypeProcessingException(
                    message: "DocumentType processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentTypeProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentTypeProcessingException.ThrowIfContainsErrors();
        }
    }
}
