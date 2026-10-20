using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;
using Paperless.Mcp.Models.Processings.DocumentTypes;
using Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions;
using Paperless.Mcp.Services.Foundations.DocumentTypes;
using Paperless.Mcp.Services.Processings.DocumentTypes;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Reines Duplex-Mapping, genau eine Service-Abhaengigkeit.
    //
    // Wie bei Tags bewusst noch kein Loesch-Tool: destruktive Operationen verlangen
    // confirm=true mit Dry-Run als Default, und diese Pruefung gehoert in eine
    // Processing Service. RemoveDocumentTypeByIdAsync existiert und ist getestet.
    [McpServerToolType]
    public class DocumentTypeTools
    {
        private readonly IDocumentTypeService documentTypeService;
        private readonly IDocumentTypeProcessingService documentTypeProcessingService;

        public DocumentTypeTools(
            IDocumentTypeService documentTypeService,
            IDocumentTypeProcessingService documentTypeProcessingService)
        {
            this.documentTypeService = documentTypeService;
            this.documentTypeProcessingService = documentTypeProcessingService;
        }

        [McpServerTool(Name = "paperless_document_types_list")]
        [Description("Lists document types from Paperless-ngx one page at a time.")]
        public async ValueTask<DocumentTypePage> ListDocumentTypesAsync(
            [Description("Page number, starting at 1.")] int page = 1,
            [Description("Number of document types per page.")] int pageSize = 25)
        {
            try
            {
                return await this.documentTypeService.RetrieveDocumentTypePageAsync(page, pageSize);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_document_types_get")]
        [Description("Gets a single document type from Paperless-ngx by its id.")]
        public async ValueTask<DocumentType> GetDocumentTypeAsync(
            [Description("Id of the document type.")] int documentTypeId)
        {
            try
            {
                return await this.documentTypeService.RetrieveDocumentTypeByIdAsync(documentTypeId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_document_types_create")]
        [Description("Creates a new document type in Paperless-ngx.")]
        public async ValueTask<DocumentType> CreateDocumentTypeAsync(
            [Description("Name of the document type, at most 128 characters.")] string name,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] DocumentTypeMatchingAlgorithm matchingAlgorithm = DocumentTypeMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true)
        {
            var documentType = new DocumentType
            {
                Name = name,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
            };

            try
            {
                return await this.documentTypeService.AddDocumentTypeAsync(documentType);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_document_types_update")]
        [Description("Replaces an existing document type in Paperless-ngx. Every field is written, omitted fields are cleared.")]
        public async ValueTask<DocumentType> UpdateDocumentTypeAsync(
            [Description("Id of the document type to replace.")] int documentTypeId,
            [Description("Name of the document type, at most 128 characters.")] string name,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] DocumentTypeMatchingAlgorithm matchingAlgorithm = DocumentTypeMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true)
        {
            var documentType = new DocumentType
            {
                Id = documentTypeId,
                Name = name,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
            };

            try
            {
                return await this.documentTypeService.ModifyDocumentTypeAsync(documentType);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_document_types_delete")]
        [Description(
            "Deletes a document type. Runs as a dry run unless confirm is true: it then "
            + "reports what would be deleted and how many documents are affected, and "
            + "changes nothing. Call again with confirm set to true to actually delete. "
            + "Documents keep existing, they only lose the type.")]
        public async ValueTask<DocumentTypeRemovalPlan> DeleteDocumentTypeAsync(
            [Description("Id of the document type.")] int documentTypeId,
            [Description("Set to true to actually delete. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.documentTypeProcessingService.RemoveDocumentTypeByIdAsync(documentTypeId, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                DocumentTypeValidationException
                    or DocumentTypeDependencyValidationException
                    or DocumentTypeProcessingValidationException
                    or DocumentTypeProcessingDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The Paperless server could not be reached or failed to answer. "
                    + "Check the server and the connection settings."),
            };

        private static string DescribeInvalidInput(Xeption exception)
        {
            Exception? inner = exception.InnerException;
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
