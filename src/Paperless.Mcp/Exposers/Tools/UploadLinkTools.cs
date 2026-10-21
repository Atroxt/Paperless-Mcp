using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.UploadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions;
using Paperless.Mcp.Services.Foundations.UploadLinks;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Nur im HTTP-Transport angemeldet. Gegenstueck zu DownloadLinkTools: grosse
    // Dateien gehen nicht als Base64 durch das Sprachmodell, sondern direkt an die
    // ausgegebene URL. Im stdio-Betrieb nimmt man paperless_documents_upload_from_path.
    [McpServerToolType]
    public class UploadLinkTools
    {
        private readonly IUploadLinkService uploadLinkService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UploadLinkConfigurations configurations;

        public UploadLinkTools(
            IUploadLinkService uploadLinkService,
            IHttpContextAccessor httpContextAccessor,
            UploadLinkConfigurations configurations)
        {
            this.uploadLinkService = uploadLinkService;
            this.httpContextAccessor = httpContextAccessor;
            this.configurations = configurations;
        }

        [McpServerTool(Name = "paperless_documents_upload_link")]
        [Description(
            "Creates a temporary upload link for a file, the right way to upload large "
            + "files (for example PDFs over a few MB) without sending them as Base64. "
            + "The file is then sent with an HTTP POST to the returned URL, either as "
            + "multipart/form-data (field 'document') or as the raw request body, e.g. "
            + "curl -F \"document=@file.pdf\" <url>. The link needs no API token, works "
            + "exactly once and expires after a few minutes. The POST answers with the "
            + "task id; use paperless_tasks_get to follow the processing.")]
        public async ValueTask<string> CreateUploadLinkAsync(
            [Description("File name including the extension, for example invoice.pdf.")] string fileName,
            [Description("Title; Paperless derives one from the file name when omitted.")] string? title = null,
            [Description("Id of the correspondent to assign.")] int? correspondentId = null,
            [Description("Id of the document type to assign.")] int? documentTypeId = null,
            [Description("Id of the storage path to assign.")] int? storagePathId = null,
            [Description("Creation date, as yyyy-MM-dd.")] string? created = null)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                throw new McpException(
                    "Upload links need the HTTP transport. Over stdio use "
                    + "paperless_documents_upload_from_path instead.");
            }

            string? token = PaperlessAuthHandler.ReadRequestToken(httpContext.Request);

            try
            {
                UploadLink uploadLink = await this.uploadLinkService.IssueUploadLinkAsync(
                    new UploadLink
                    {
                        FileName = fileName,
                        Title = title,
                        CorrespondentId = correspondentId,
                        DocumentTypeId = documentTypeId,
                        StoragePathId = storagePathId,
                        Created = ParseDate(created),
                        Token = token ?? string.Empty,
                    });

                return $"{ResolveBaseUrl(httpContext)}"
                    + $"{UploadLinkConfigurations.RoutePrefix}/{uploadLink.Id}";
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private string ResolveBaseUrl(HttpContext httpContext) =>
            string.IsNullOrWhiteSpace(this.configurations.PublicUrl) is false
                ? this.configurations.PublicUrl.TrimEnd('/')
                : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
                  + $"{httpContext.Request.PathBase}";

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
                UploadLinkValidationException
                    or UploadLinkDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The upload link could not be created. See the server log."),
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
