using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Processings.CustomFields;
using Paperless.Mcp.Services.Foundations.CustomFields;

namespace Paperless.Mcp.Services.Processings.CustomFields
{
    // Hoeherwertige Logik einer Entity: holt erst den CustomField, um die Tragweite zu kennen,
    // und entfernt ihn nur bei ausdruecklicher Bestaetigung. Zwei Schritte auf einer
    // Foundation Service -- genau das, wofuer die Processing-Schicht da ist.
    public partial class CustomFieldProcessingService : ICustomFieldProcessingService
    {
        private readonly ICustomFieldService customFieldService;

        public CustomFieldProcessingService(ICustomFieldService customFieldService) =>
            this.customFieldService = customFieldService;

        public ValueTask<CustomFieldRemovalPlan> RemoveCustomFieldByIdAsync(int customFieldId, bool confirm) =>
            TryCatch(async () =>
            {
                ValidateCustomFieldIdOnRemove(customFieldId);

                // Immer zuerst lesen: der Bericht soll die Tragweite nennen, und zwar
                // auch dann, wenn anschliessend geloescht wird.
                CustomField customField = await this.customFieldService.RetrieveCustomFieldByIdAsync(customFieldId);

                if (confirm is false)
                {
                    return CreateDryRunPlan(customField);
                }

                await this.customFieldService.RemoveCustomFieldByIdAsync(customFieldId);

                return CreateRemovedPlan(customField);
            });

        private static CustomFieldRemovalPlan CreateDryRunPlan(CustomField customField) =>
            new()
            {
                CustomFieldId = customField.Id,
                Name = customField.Name,
                DocumentCount = customField.DocumentCount,
                Removed = false,

                Summary =
                    $"Dry run: customField '{customField.Name}' would be deleted. "
                    + $"{DescribeDocumentCount(customField.DocumentCount)} "
                    + "Call again with confirm set to true to actually delete it.",
            };

        private static CustomFieldRemovalPlan CreateRemovedPlan(CustomField customField) =>
            new()
            {
                CustomFieldId = customField.Id,
                Name = customField.Name,
                DocumentCount = customField.DocumentCount,
                Removed = true,

                Summary =
                    $"CustomField '{customField.Name}' was deleted. "
                    + $"{DescribeDocumentCount(customField.DocumentCount)}",
            };

        private static string DescribeDocumentCount(int documentCount) =>
            documentCount is 0
                ? "No document has a value for it."
                : $"{documentCount} document(s) have a value for it, and those values "
                  + "are removed with the field; the documents themselves are kept.";
    }
}
