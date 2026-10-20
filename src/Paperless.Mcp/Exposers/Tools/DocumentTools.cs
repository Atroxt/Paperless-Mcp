using System.ComponentModel;
using System.Globalization;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Processings.Documents;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Ausnahme von der Ein-Abhaengigkeit-Regel? Nein: die Suche geht ueber die
    // Processing Service (Seitengroesse begrenzen), der Einzelabruf braucht das nicht
    // und geht direkt an die Foundation Service. Beides sind Services, kein Broker, und
    // jede Methode hat genau einen Ansprechpartner.
    [McpServerToolType]
    public class DocumentTools
    {
        private readonly IDocumentProcessingService documentProcessingService;
        private readonly IDocumentService documentService;

        public DocumentTools(
            IDocumentProcessingService documentProcessingService,
            IDocumentService documentService)
        {
            this.documentProcessingService = documentProcessingService;
            this.documentService = documentService;
        }

        [McpServerTool(Name = "paperless_documents_search")]
        [Description(
            "Searches documents in Paperless-ngx. All filters are optional and combine "
            + "with AND. Returns one page; the page size is capped by the server.")]
        public async ValueTask<DocumentPage> SearchDocumentsAsync(
            [Description("Full text search across content and metadata.")] string? query = null,
            [Description("Case-insensitive substring of the title.")] string? titleContains = null,
            [Description("Id of the correspondent.")] int? correspondentId = null,
            [Description("Id of the document type.")] int? documentTypeId = null,
            [Description("Id of the storage path.")] int? storagePathId = null,
            [Description("Ids of tags; only documents carrying all of them match.")] int[]? tagIds = null,
            [Description("Earliest creation date, as yyyy-MM-dd.")] string? createdFrom = null,
            [Description("Latest creation date, as yyyy-MM-dd.")] string? createdTo = null,
            [Description("Sort field, one of id, title, created, added, modified, correspondent__name, document_type__name, archive_serial_number. Prefix with - for descending.")] string? ordering = null,
            [Description("Page number, starting at 1.")] int page = 1,
            [Description("Documents per page.")] int pageSize = 25)
        {
            var documentSearch = new DocumentSearch
            {
                Query = query,
                TitleContains = titleContains,
                CorrespondentId = correspondentId,
                DocumentTypeId = documentTypeId,
                StoragePathId = storagePathId,
                TagIds = tagIds is null ? [] : [.. tagIds],
                CreatedFrom = ParseDate(createdFrom, nameof(createdFrom)),
                CreatedTo = ParseDate(createdTo, nameof(createdTo)),
                Ordering = ordering,
                Page = page,
                PageSize = pageSize,
            };

            try
            {
                return await this.documentProcessingService.SearchDocumentsAsync(documentSearch);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_documents_get")]
        [Description("Gets a single document from Paperless-ngx by its id, including its extracted text.")]
        public async ValueTask<Document> GetDocumentAsync(
            [Description("Id of the document.")] int documentId)
        {
            try
            {
                return await this.documentService.RetrieveDocumentByIdAsync(documentId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }


        [McpServerTool(Name = "paperless_documents_update")]
        [Description(
            "Changes a document in Paperless-ngx. Only the fields given are written, "
            + "everything else stays as it is, including the extracted text. Clearing a "
            + "correspondent, document type or storage path is not supported yet.")]
        public async ValueTask<Document> UpdateDocumentAsync(
            [Description("Id of the document to change.")] int documentId,
            [Description("New title.")] string? title = null,
            [Description("Id of the correspondent to assign.")] int? correspondentId = null,
            [Description("Id of the document type to assign.")] int? documentTypeId = null,
            [Description("Id of the storage path to assign.")] int? storagePathId = null,
            [Description("Tag ids; replaces all tags. An empty array removes every tag.")] int[]? tagIds = null,
            [Description("Creation date, as yyyy-MM-dd.")] string? created = null,
            [Description("Archive serial number.")] int? archiveSerialNumber = null)
        {
            var documentUpdate = new DocumentUpdate
            {
                DocumentId = documentId,
                Title = title,
                CorrespondentId = correspondentId,
                DocumentTypeId = documentTypeId,
                StoragePathId = storagePathId,
                TagIds = tagIds is null ? null : [.. tagIds],
                Created = ParseDate(created, nameof(created)),
                ArchiveSerialNumber = archiveSerialNumber,
            };

            try
            {
                return await this.documentService.ModifyDocumentAsync(documentUpdate);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_documents_set_custom_fields")]
        [Description(
            "Sets the custom field values of a document. Replaces all values at once: "
            + "fields not listed are removed from the document. Use "
            + "paperless_custom_fields_list to find field ids.")]
        public async ValueTask<Document> SetCustomFieldsAsync(
            [Description("Id of the document.")] int documentId,
            [Description("Ids of the custom fields, in the same order as values.")] int[] fieldIds,
            [Description("Values for those fields, as text. Paperless converts them to the field's type; a monetary field expects something like EUR123.45.")] string[] values)
        {
            if (fieldIds.Length != values.Length)
            {
                throw new McpException(
                    $"fieldIds and values must have the same length; got "
                    + $"{fieldIds.Length} and {values.Length}.");
            }

            var documentUpdate = new DocumentUpdate
            {
                DocumentId = documentId,

                CustomFields = [.. fieldIds.Zip(values, (fieldId, value) =>
                    new DocumentCustomFieldValue { Field = fieldId, Value = value })],
            };

            try
            {
                return await this.documentService.ModifyDocumentAsync(documentUpdate);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }


        [McpServerTool(Name = "paperless_documents_upload")]
        [Description(
            "Uploads a file to Paperless-ngx. Paperless processes it in the background "
            + "and answers with a task id, not with a document; use paperless_tasks_get "
            + "with that id to see when the document exists and which id it got.")]
        public async ValueTask<string> UploadDocumentAsync(
            [Description("File name including the extension, for example rechnung.pdf.")] string fileName,
            [Description("File content, Base64 encoded.")] string contentBase64,
            [Description("Title; Paperless derives one from the file name when omitted.")] string? title = null,
            [Description("Id of the correspondent to assign.")] int? correspondentId = null,
            [Description("Id of the document type to assign.")] int? documentTypeId = null,
            [Description("Id of the storage path to assign.")] int? storagePathId = null,
            [Description("Creation date, as yyyy-MM-dd.")] string? created = null)
        {
            byte[] content;

            try
            {
                content = Convert.FromBase64String(contentBase64);
            }
            catch (FormatException)
            {
                throw new McpException("contentBase64 is not valid Base64.");
            }

            var documentUpload = new DocumentUpload
            {
                FileName = fileName,
                Content = content,
                Title = title,
                CorrespondentId = correspondentId,
                DocumentTypeId = documentTypeId,
                StoragePathId = storagePathId,
                Created = ParseDate(created, nameof(created)),
            };

            try
            {
                return await this.documentService.AddDocumentFileAsync(documentUpload);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_documents_thumbnail")]
        [Description("Returns the thumbnail of a document as an image.")]
        public async ValueTask<ImageContentBlock> GetThumbnailAsync(
            [Description("Id of the document.")] int documentId)
        {
            try
            {
                DocumentThumbnail thumbnail =
                    await this.documentService.RetrieveDocumentThumbnailAsync(documentId);

                return new ImageContentBlock
                {
                    Data = thumbnail.Content,
                    MimeType = thumbnail.MimeType,
                };
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_documents_delete")]
        [Description(
            "Deletes a document including its file and extracted text. This cannot be "
            + "undone. Runs as a dry run unless confirm is true: it then reports which "
            + "document would be deleted and changes nothing. Call again with confirm "
            + "set to true to actually delete.")]
        public async ValueTask<DocumentRemovalPlan> DeleteDocumentAsync(
            [Description("Id of the document.")] int documentId,
            [Description("Set to true to actually delete. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.documentProcessingService.RemoveDocumentByIdAsync(
                    documentId, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_documents_reprocess")]
        [Description(
            "Has Paperless read a document again, regenerating its extracted text and "
            + "archive file. Manual corrections to the text are lost. Runs as a dry run "
            + "unless confirm is true. Paperless then works in the background; use "
            + "paperless_documents_get to see the result.")]
        public async ValueTask<DocumentReprocessPlan> ReprocessDocumentAsync(
            [Description("Id of the document.")] int documentId,
            [Description("Set to true to actually start it. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.documentProcessingService.ReprocessDocumentByIdAsync(
                    documentId, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        // Datumsangaben kommen als Zeichenkette herein, damit das Werkzeugschema ein
        // einfaches Format vorgibt. Ein unlesbares Datum ist ein Eingabefehler und wird
        // sofort als solcher gemeldet.
        private static DateOnly? ParseDate(string? value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (DateOnly.TryParseExact(
                    value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateOnly parsedDate))
            {
                return parsedDate;
            }

            throw new McpException($"{parameterName} must be a date in the form yyyy-MM-dd.");
        }

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
            // Die Processing-Schicht verpackt den eigentlichen Grund eine Ebene tiefer.
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
