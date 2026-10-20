using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;

namespace Paperless.Mcp.Services.Processings.Documents
{
    public partial class DocumentProcessingService
    {
        // Processing Services validieren nur, was sie selbst verwenden. Alles andere am
        // Suchmodell prueft die Foundation Service.
        private static void ValidateDocumentSearchOnSearch(DocumentSearch documentSearch)
        {
            ValidateDocumentSearchIsNotNull(documentSearch);

            Validate((Rule: IsInvalid(documentSearch.PageSize),
                Parameter: nameof(DocumentSearch.PageSize)));
        }

        private static void ValidateDocumentIdOnAct(int documentId) =>
            Validate((Rule: IsInvalid(documentId), Parameter: nameof(Document.Id)));

        private static void ValidateDocumentSearchIsNotNull(DocumentSearch documentSearch)
        {
            if (documentSearch is null)
            {
                throw new NullDocumentProcessingException(message: "DocumentSearch is null.");
            }
        }

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentProcessingException =
                new InvalidDocumentProcessingException(
                    message: "Document processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentProcessingException.ThrowIfContainsErrors();
        }
    }
}
