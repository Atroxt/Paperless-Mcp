using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Processings.Correspondents;
using Paperless.Mcp.Services.Foundations.Correspondents;

namespace Paperless.Mcp.Services.Processings.Correspondents
{
    // Hoeherwertige Logik einer Entity: holt erst den Correspondent, um die Tragweite zu kennen,
    // und entfernt ihn nur bei ausdruecklicher Bestaetigung. Zwei Schritte auf einer
    // Foundation Service -- genau das, wofuer die Processing-Schicht da ist.
    public partial class CorrespondentProcessingService : ICorrespondentProcessingService
    {
        private readonly ICorrespondentService correspondentService;

        public CorrespondentProcessingService(ICorrespondentService correspondentService) =>
            this.correspondentService = correspondentService;

        public ValueTask<CorrespondentRemovalPlan> RemoveCorrespondentByIdAsync(int correspondentId, bool confirm) =>
            TryCatch(async () =>
            {
                ValidateCorrespondentIdOnRemove(correspondentId);

                // Immer zuerst lesen: der Bericht soll die Tragweite nennen, und zwar
                // auch dann, wenn anschliessend geloescht wird.
                Correspondent correspondent = await this.correspondentService.RetrieveCorrespondentByIdAsync(correspondentId);

                if (confirm is false)
                {
                    return CreateDryRunPlan(correspondent);
                }

                await this.correspondentService.RemoveCorrespondentByIdAsync(correspondentId);

                return CreateRemovedPlan(correspondent);
            });

        private static CorrespondentRemovalPlan CreateDryRunPlan(Correspondent correspondent) =>
            new()
            {
                CorrespondentId = correspondent.Id,
                Name = correspondent.Name,
                DocumentCount = correspondent.DocumentCount,
                Removed = false,

                Summary =
                    $"Dry run: correspondent '{correspondent.Name}' would be deleted. "
                    + $"{DescribeDocumentCount(correspondent.DocumentCount)} "
                    + "Call again with confirm set to true to actually delete it.",
            };

        private static CorrespondentRemovalPlan CreateRemovedPlan(Correspondent correspondent) =>
            new()
            {
                CorrespondentId = correspondent.Id,
                Name = correspondent.Name,
                DocumentCount = correspondent.DocumentCount,
                Removed = true,

                Summary =
                    $"Correspondent '{correspondent.Name}' was deleted. "
                    + $"{DescribeDocumentCount(correspondent.DocumentCount)}",
            };

        private static string DescribeDocumentCount(int documentCount) =>
            documentCount is 0
                ? "No document is assigned to it."
                : $"{documentCount} document(s) are assigned to it and lose the "
                  + "assignment; the documents themselves are kept.";
    }
}
