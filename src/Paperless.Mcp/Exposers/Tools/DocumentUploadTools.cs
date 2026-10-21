using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Orchestrations.Documents;
using Paperless.Mcp.Models.Orchestrations.Documents.Exceptions;
using Paperless.Mcp.Services.Orchestrations.Documents;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Genau eine Abhaengigkeit, und die ist eine Orchestration Service: das Warten und
    // das Nachlesen des Dokuments passieren dort, nicht hier.
    [McpServerToolType]
    public class DocumentUploadTools
    {
        private readonly IDocumentOrchestrationService documentOrchestrationService;

        public DocumentUploadTools(
            IDocumentOrchestrationService documentOrchestrationService) =>
            this.documentOrchestrationService = documentOrchestrationService;

        [McpServerTool(Name = "paperless_documents_upload_and_wait")]
        [Description(
            "Uploads a file and waits until Paperless has processed it, then returns the "
            + "new document. Prefer this over paperless_documents_upload when you want to "
            + "work with the document afterwards. If processing takes longer than the "
            + "timeout, the upload keeps running and the result carries the task id to "
            + "follow it with paperless_tasks_get. Only for small files: for anything "
            + "larger than a few MB use paperless_documents_upload_link (HTTP) or "
            + "paperless_documents_upload_from_path (stdio).")]
        public async ValueTask<DocumentUploadResult> AddDocumentAndAwaitAsync(
            [Description("File content, Base64 encoded.")] string contentBase64,
            [Description("File name including the extension, for example invoice.pdf.")] string fileName,
            [Description("Title of the document. Paperless derives one from the file name if omitted.")] string? title = null,
            [Description("Id of the correspondent.")] int? correspondentId = null,
            [Description("Id of the document type.")] int? documentTypeId = null,
            [Description("Id of the storage path.")] int? storagePathId = null,
            [Description("Creation date, as yyyy-MM-dd.")] string? created = null,
            [Description("How many seconds to wait, 1 to 120.")] int timeoutSeconds = 30)
        {
            byte[] content;

            try
            {
                content = Convert.FromBase64String(contentBase64 ?? string.Empty);
            }
            catch (FormatException)
            {
                throw new McpException("contentBase64 is not valid Base64.");
            }

            var documentUpload = new DocumentUpload
            {
                Content = content,
                FileName = fileName,
                Title = title,
                CorrespondentId = correspondentId,
                DocumentTypeId = documentTypeId,
                StoragePathId = storagePathId,
                Created = ParseDate(created),
            };

            try
            {
                return await this.documentOrchestrationService.AddDocumentAndAwaitAsync(
                    documentUpload, timeoutSeconds);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static DateOnly? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateOnly.TryParse(
                value,
                System.Globalization.CultureInfo.InvariantCulture,
                out DateOnly parsed)
                ? parsed
                : throw new McpException("created must be a date in the form yyyy-MM-dd.");
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                DocumentValidationException
                    or DocumentDependencyValidationException
                    or DocumentOrchestrationValidationException
                    or DocumentOrchestrationDependencyValidationException =>
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
