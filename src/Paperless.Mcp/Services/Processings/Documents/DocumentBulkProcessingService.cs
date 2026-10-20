using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Services.Foundations.Documents;

namespace Paperless.Mcp.Services.Processings.Documents
{
    // Hoeherwertige Logik einer Entity: setzt eine fachliche Sammeloperation aus mehreren
    // bulk_edit-Aufrufen zusammen und erzwingt die Bestaetigung. Genau eine Foundation
    // Service als Abhaengigkeit.
    //
    // Hier und nicht im Werkzeug sitzt der Guard -- ein zweites Werkzeug, das dieselbe
    // Foundation Service nutzt, wuerde ihn sonst umgehen.
    public partial class DocumentBulkProcessingService : IDocumentBulkProcessingService
    {
        private readonly IDocumentService documentService;

        public DocumentBulkProcessingService(IDocumentService documentService) =>
            this.documentService = documentService;

        public ValueTask<DocumentBulkPlan> BulkUpdateDocumentsAsync(
            DocumentBulkUpdate documentBulkUpdate,
            bool confirm) =>
            TryCatch(async () =>
            {
                ValidateDocumentBulkUpdate(documentBulkUpdate);

                DocumentBulkPlan plan = await CreatePlanAsync(
                    documentBulkUpdate.DocumentIds,
                    DescribeChanges(documentBulkUpdate));

                if (confirm is false)
                {
                    plan.Summary =
                        $"Dry run: {Describe(plan)} would be changed. "
                        + "Call again with confirm set to true to apply it.";

                    return plan;
                }

                if (plan.AffectedDocumentIds.Count is 0)
                {
                    plan.Summary =
                        "Nothing was changed: none of the requested documents exist.";

                    return plan;
                }

                await ApplyUpdateAsync(documentBulkUpdate, plan.AffectedDocumentIds);

                plan.Applied = true;
                plan.Summary = $"{Describe(plan)} were changed.";

                return plan;
            });

        public ValueTask<DocumentBulkPlan> BulkRemoveDocumentsAsync(
            List<int> documentIds,
            bool confirm) =>
            TryCatch(async () =>
            {
                ValidateDocumentIdsOnAct(documentIds);

                DocumentBulkPlan plan = await CreatePlanAsync(
                    documentIds,
                    changes:
                    [
                        "Deleted, including the file and the extracted text",
                    ]);

                if (confirm is false)
                {
                    plan.Summary =
                        $"Dry run: {Describe(plan)} would be deleted, including their "
                        + "files and extracted text. This cannot be undone. Call again "
                        + "with confirm set to true to delete them.";

                    return plan;
                }

                if (plan.AffectedDocumentIds.Count is 0)
                {
                    plan.Summary =
                        "Nothing was deleted: none of the requested documents exist.";

                    return plan;
                }

                await this.documentService.RemoveDocumentsAsync(plan.AffectedDocumentIds);

                plan.Applied = true;
                plan.Summary = $"{Describe(plan)} were deleted.";

                return plan;
            });

        // Erst lesen, immer -- auch im bestaetigten Fall. Genau diese Zahl macht eine
        // Bestaetigung zu einer Entscheidung, und nur so werden unbekannte Ids sichtbar.
        private async ValueTask<DocumentBulkPlan> CreatePlanAsync(
            List<int> documentIds,
            List<string> changes)
        {
            DocumentPage affectedPage =
                await this.documentService.RetrieveDocumentPageAsync(new DocumentSearch
                {
                    DocumentIds = documentIds,
                    PageSize = documentIds.Count,
                });

            List<int> affectedIds = affectedPage.Results
                .Select(document => document.Id)
                .ToList();

            return new DocumentBulkPlan
            {
                RequestedDocumentIds = documentIds,
                AffectedDocumentIds = affectedIds,

                UnknownDocumentIds = documentIds
                    .Where(documentId => affectedIds.Contains(documentId) is false)
                    .ToList(),

                AffectedTitles = affectedPage.Results
                    .Select(document => document.Title)
                    .ToList(),

                Changes = changes,
                Applied = false,
            };
        }

        // Die Reihenfolge ist festgelegt, nicht zufaellig: jeder Schritt ist ein eigener
        // Aufruf, und bricht einer ab, soll nachvollziehbar sein, was schon geschehen ist.
        private async ValueTask ApplyUpdateAsync(
            DocumentBulkUpdate documentBulkUpdate,
            List<int> documentIds)
        {
            if (documentBulkUpdate.Correspondent is not null)
            {
                await this.documentService.ModifyDocumentsCorrespondentAsync(
                    documentIds,
                    ResolveReference(documentBulkUpdate.Correspondent));
            }

            if (documentBulkUpdate.DocumentType is not null)
            {
                await this.documentService.ModifyDocumentsDocumentTypeAsync(
                    documentIds,
                    ResolveReference(documentBulkUpdate.DocumentType));
            }

            if (documentBulkUpdate.StoragePath is not null)
            {
                await this.documentService.ModifyDocumentsStoragePathAsync(
                    documentIds,
                    ResolveReference(documentBulkUpdate.StoragePath));
            }

            if (ChangesTags(documentBulkUpdate))
            {
                await this.documentService.ModifyDocumentsTagsAsync(
                    documentIds,
                    documentBulkUpdate.AddTagIds,
                    documentBulkUpdate.RemoveTagIds);
            }
        }

        // Dreiwertig nach zweiwertig: die Foundation Service kennt nur "setzen" und
        // "leeren", weil die API nur das kennt. Ob ueberhaupt etwas geschieht, hat die
        // Schicht darueber schon entschieden.
        private static int? ResolveReference(DocumentReferenceUpdate referenceUpdate) =>
            referenceUpdate.Clear ? null : referenceUpdate.Id;

        private static bool ChangesTags(DocumentBulkUpdate documentBulkUpdate) =>
            documentBulkUpdate.AddTagIds.Count > 0
            || documentBulkUpdate.RemoveTagIds.Count > 0;

        private static List<string> DescribeChanges(DocumentBulkUpdate documentBulkUpdate)
        {
            var changes = new List<string>();

            Describe("Correspondent", documentBulkUpdate.Correspondent);
            Describe("Document type", documentBulkUpdate.DocumentType);
            Describe("Storage path", documentBulkUpdate.StoragePath);

            if (documentBulkUpdate.AddTagIds.Count > 0)
            {
                changes.Add($"Tags added: {string.Join(", ", documentBulkUpdate.AddTagIds)}");
            }

            if (documentBulkUpdate.RemoveTagIds.Count > 0)
            {
                changes.Add($"Tags removed: {string.Join(", ", documentBulkUpdate.RemoveTagIds)}");
            }

            return changes;

            void Describe(string label, DocumentReferenceUpdate? referenceUpdate)
            {
                if (referenceUpdate is null)
                {
                    return;
                }

                changes.Add(referenceUpdate.Clear
                    ? $"{label} cleared"
                    : $"{label} set to {referenceUpdate.Id}");
            }
        }

        private static string Describe(DocumentBulkPlan plan) =>
            plan.UnknownDocumentIds.Count is 0
                ? $"{plan.AffectedDocumentIds.Count} document(s)"
                : $"{plan.AffectedDocumentIds.Count} of {plan.RequestedDocumentIds.Count} "
                  + $"requested document(s) ({plan.UnknownDocumentIds.Count} do not exist: "
                  + $"{string.Join(", ", plan.UnknownDocumentIds)})";
    }
}
