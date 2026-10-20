using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;

namespace Paperless.Mcp.Services.Processings.Documents
{
    public partial class DocumentFileProcessingService
    {
        private static void ValidateFilePath(string filePath) =>
            Validate((Rule: IsInvalid(filePath), Parameter: "FilePath"));

        private static void ValidateDocumentIdAndTargetPath(int documentId, string targetPath) =>
            Validate(
                (Rule: IsInvalid(documentId), Parameter: nameof(Document.Id)),
                (Rule: IsInvalid(targetPath), Parameter: "TargetPath"));

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
