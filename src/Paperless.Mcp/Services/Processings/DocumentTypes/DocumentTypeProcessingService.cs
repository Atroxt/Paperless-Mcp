using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Processings.DocumentTypes;
using Paperless.Mcp.Services.Foundations.DocumentTypes;

namespace Paperless.Mcp.Services.Processings.DocumentTypes
{
    // Hoeherwertige Logik einer Entity: holt erst den DocumentType, um die Tragweite zu kennen,
    // und entfernt ihn nur bei ausdruecklicher Bestaetigung. Zwei Schritte auf einer
    // Foundation Service -- genau das, wofuer die Processing-Schicht da ist.
    public partial class DocumentTypeProcessingService : IDocumentTypeProcessingService
    {
        private readonly IDocumentTypeService documentTypeService;

        public DocumentTypeProcessingService(IDocumentTypeService documentTypeService) =>
            this.documentTypeService = documentTypeService;

        public ValueTask<DocumentTypeRemovalPlan> RemoveDocumentTypeByIdAsync(int documentTypeId, bool confirm) =>
            TryCatch(async () =>
            {
                ValidateDocumentTypeIdOnRemove(documentTypeId);

                // Immer zuerst lesen: der Bericht soll die Tragweite nennen, und zwar
                // auch dann, wenn anschliessend geloescht wird.
                DocumentType documentType = await this.documentTypeService.RetrieveDocumentTypeByIdAsync(documentTypeId);

                if (confirm is false)
                {
                    return CreateDryRunPlan(documentType);
                }

                await this.documentTypeService.RemoveDocumentTypeByIdAsync(documentTypeId);

                return CreateRemovedPlan(documentType);
            });

        private static DocumentTypeRemovalPlan CreateDryRunPlan(DocumentType documentType) =>
            new()
            {
                DocumentTypeId = documentType.Id,
                Name = documentType.Name,
                DocumentCount = documentType.DocumentCount,
                Removed = false,

                Summary =
                    $"Dry run: documentType '{documentType.Name}' would be deleted. "
                    + $"{DescribeDocumentCount(documentType.DocumentCount)} "
                    + "Call again with confirm set to true to actually delete it.",
            };

        private static DocumentTypeRemovalPlan CreateRemovedPlan(DocumentType documentType) =>
            new()
            {
                DocumentTypeId = documentType.Id,
                Name = documentType.Name,
                DocumentCount = documentType.DocumentCount,
                Removed = true,

                Summary =
                    $"DocumentType '{documentType.Name}' was deleted. "
                    + $"{DescribeDocumentCount(documentType.DocumentCount)}",
            };

        private static string DescribeDocumentCount(int documentCount) =>
            documentCount is 0
                ? "No document uses it."
                : $"{documentCount} document(s) use it and lose the type; "
                  + "the documents themselves are kept.";
    }
}
