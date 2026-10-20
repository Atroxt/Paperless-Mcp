using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;
using Paperless.Mcp.Services.Processings.Documents;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Sammeloperationen gehen ausschliesslich ueber die Processing Service: dort sitzt
    // der confirm-Guard. Genau eine Abhaengigkeit, reines Mapping.
    //
    // Die Dreiwertigkeit einer Zuordnung wird hier flach ausgelegt -- je Zuordnung eine
    // Id und ein clear-Schalter. Ein verschachteltes Objekt waere im Werkzeugschema
    // schwerer zu treffen, und wer das Schema nicht versteht, loescht sonst versehentlich
    // einen Korrespondenten statt ihn zu setzen.
    [McpServerToolType]
    public class DocumentBulkTools
    {
        private readonly IDocumentBulkProcessingService documentBulkProcessingService;

        public DocumentBulkTools(
            IDocumentBulkProcessingService documentBulkProcessingService) =>
            this.documentBulkProcessingService = documentBulkProcessingService;

        [McpServerTool(Name = "paperless_documents_bulk_update")]
        [Description(
            "Changes correspondent, document type, storage path or tags on many documents "
            + "at once. Runs as a dry run unless confirm is true: it then reports how many "
            + "documents are affected, which of the given ids do not exist, and what would "
            + "change. Call again with confirm set to true to apply it. At most 100 "
            + "documents per call.")]
        public async ValueTask<DocumentBulkPlan> BulkUpdateDocumentsAsync(
            [Description("Ids of the documents to change.")] int[] documentIds,
            [Description("Id of the correspondent to assign.")] int? correspondentId = null,
            [Description("Set to true to remove the correspondent instead of assigning one.")] bool clearCorrespondent = false,
            [Description("Id of the document type to assign.")] int? documentTypeId = null,
            [Description("Set to true to remove the document type instead of assigning one.")] bool clearDocumentType = false,
            [Description("Id of the storage path to assign.")] int? storagePathId = null,
            [Description("Set to true to remove the storage path instead of assigning one.")] bool clearStoragePath = false,
            [Description("Ids of tags to add to every document.")] int[]? addTagIds = null,
            [Description("Ids of tags to remove from every document.")] int[]? removeTagIds = null,
            [Description("Set to true to actually apply the change. Leave false to see what would happen.")] bool confirm = false)
        {
            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = documentIds is null ? [] : [.. documentIds],
                Correspondent = MapReference(correspondentId, clearCorrespondent),
                DocumentType = MapReference(documentTypeId, clearDocumentType),
                StoragePath = MapReference(storagePathId, clearStoragePath),
                AddTagIds = addTagIds is null ? [] : [.. addTagIds],
                RemoveTagIds = removeTagIds is null ? [] : [.. removeTagIds],
            };

            try
            {
                return await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_documents_bulk_delete")]
        [Description(
            "Deletes many documents at once, including their files and extracted text. "
            + "This cannot be undone. Runs as a dry run unless confirm is true: it then "
            + "reports how many documents would be deleted and names them. Call again "
            + "with confirm set to true to actually delete. At most 100 documents per call.")]
        public async ValueTask<DocumentBulkPlan> BulkDeleteDocumentsAsync(
            [Description("Ids of the documents to delete.")] int[] documentIds,
            [Description("Set to true to actually delete. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.documentBulkProcessingService.BulkRemoveDocumentsAsync(
                    documentIds is null ? [] : [.. documentIds], confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        // Nicht gesetzt und nicht geleert heisst: diese Zuordnung bleibt unberuehrt.
        private static DocumentReferenceUpdate? MapReference(int? id, bool clear) =>
            id is null && clear is false
                ? null
                : new DocumentReferenceUpdate { Id = id, Clear = clear };

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                DocumentValidationException
                    or DocumentDependencyValidationException
                    or DocumentProcessingValidationException
                    or DocumentProcessingDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The Paperless server could not be reached or failed to answer. "
                    + "Check the server and the connection settings."),
            };

        private static string DescribeInvalidInput(Xeption exception)
        {
            Exception? inner = exception.InnerException;

            while (inner is not null && inner.Data.Count is 0 && inner.InnerException is not null)
            {
                inner = inner.InnerException;
            }

            string reason = inner?.Message ?? exception.Message;

            if (inner is null || inner.Data.Count is 0)
            {
                return reason;
            }

            IEnumerable<string> details = inner.Data.Keys
                .Cast<object>()
                .Select(key => $"{key}: {Describe(inner.Data[key])}");

            return $"{reason} {string.Join("; ", details)}";
        }

        private static string Describe(object? value) =>
            value switch
            {
                IEnumerable<string> messages => string.Join(", ", messages),
                null => string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
    }
}
