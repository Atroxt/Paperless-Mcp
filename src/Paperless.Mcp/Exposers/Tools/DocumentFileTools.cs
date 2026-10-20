using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;
using Paperless.Mcp.Services.Processings.Documents;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Diese Werkzeuge fassen das lokale Dateisystem an und werden deshalb NUR im
    // stdio-Transport angemeldet, siehe Program.cs. Im HTTP-Transport erscheinen sie
    // gar nicht erst in tools/list -- der DocumentFileProcessingService prueft es
    // zusaetzlich selbst.
    [McpServerToolType]
    public class DocumentFileTools
    {
        private readonly IDocumentFileProcessingService documentFileProcessingService;

        public DocumentFileTools(IDocumentFileProcessingService documentFileProcessingService) =>
            this.documentFileProcessingService = documentFileProcessingService;

        [McpServerTool(Name = "paperless_documents_upload_from_path")]
        [Description(
            "Uploads a file from the local file system to Paperless-ngx. The path is "
            + "read on the machine the MCP server runs on. Answers with a task id; use "
            + "paperless_tasks_get to follow the processing.")]
        public async ValueTask<string> UploadFromPathAsync(
            [Description("Full path of the file to upload.")] string filePath,
            [Description("Title; Paperless derives one from the file name when omitted.")] string? title = null,
            [Description("Id of the correspondent to assign.")] int? correspondentId = null,
            [Description("Id of the document type to assign.")] int? documentTypeId = null,
            [Description("Id of the storage path to assign.")] int? storagePathId = null)
        {
            try
            {
                return await this.documentFileProcessingService.UploadFromPathAsync(
                    filePath, title, correspondentId, documentTypeId, storagePathId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_documents_download_to_path")]
        [Description(
            "Downloads a document and writes it to the local file system. An existing "
            + "file at that path is overwritten. Returns the number of bytes written.")]
        public async ValueTask<long> DownloadToPathAsync(
            [Description("Id of the document.")] int documentId,
            [Description("Full path to write to; an existing file is overwritten.")] string targetPath,
            [Description("True for the original file, false for the archived version.")] bool original = false)
        {
            try
            {
                return await this.documentFileProcessingService.DownloadToPathAsync(
                    documentId, targetPath, original);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                DocumentProcessingValidationException
                    or DocumentProcessingDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The Paperless server could not be reached or the file could not be "
                    + "written. Check the server, the connection settings and the path."),
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
