using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;
using Paperless.Mcp.Services.Foundations.DownloadLinks;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Nur im HTTP-Transport angemeldet: ein Link ist nur dann etwas wert, wenn es auch
    // einen Server gibt, der ihn bedient. Im stdio-Betrieb nimmt man
    // paperless_documents_download_to_path.
    //
    // Der Exposer setzt die absolute URL zusammen -- er kennt Schema, Host und Pfadbasis
    // der Anfrage, der Dienst nicht. Genau dafuer ist die Exposure-Schicht da.
    [McpServerToolType]
    public class DownloadLinkTools
    {
        private readonly IDownloadLinkService downloadLinkService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly DownloadLinkConfigurations configurations;

        public DownloadLinkTools(
            IDownloadLinkService downloadLinkService,
            IHttpContextAccessor httpContextAccessor,
            DownloadLinkConfigurations configurations)
        {
            this.downloadLinkService = downloadLinkService;
            this.httpContextAccessor = httpContextAccessor;
            this.configurations = configurations;
        }

        [McpServerTool(Name = "paperless_documents_download_link")]
        [Description(
            "Creates a temporary download link for a document. The link works without an "
            + "API token, can be used exactly once and expires after a few minutes. Hand "
            + "it to the user instead of the file content; it is the right way to get a "
            + "document out of a hosted server.")]
        public async ValueTask<string> CreateDownloadLinkAsync(
            [Description("Id of the document.")] int documentId,
            [Description("True for the original file, false for the archived version.")] bool original = false)
        {
            HttpContext? httpContext = this.httpContextAccessor.HttpContext;

            if (httpContext is null)
            {
                throw new McpException(
                    "Download links need the HTTP transport. Over stdio use "
                    + "paperless_documents_download_to_path instead.");
            }

            // Das Token des Aufrufers wandert in den Link: der spaetere Download laeuft
            // mit SEINEN Rechten, nicht mit denen des Betreibers.
            string? token = PaperlessAuthHandler.ReadRequestToken(httpContext.Request);

            try
            {
                DownloadLink downloadLink =
                    await this.downloadLinkService.IssueDownloadLinkAsync(
                        documentId, original, token ?? string.Empty);

                return $"{ResolveBaseUrl(httpContext)}"
                    + $"{DownloadLinkConfigurations.RoutePrefix}/{downloadLink.Id}";
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        // Hinter einem Reverse Proxy weicht die Adresse aus dem Request von der ab, die
        // der Aufrufer kennt. Dann zaehlt die konfigurierte.
        private string ResolveBaseUrl(HttpContext httpContext) =>
            string.IsNullOrWhiteSpace(this.configurations.PublicUrl) is false
                ? this.configurations.PublicUrl.TrimEnd('/')
                : $"{httpContext.Request.Scheme}://{httpContext.Request.Host}"
                  + $"{httpContext.Request.PathBase}";

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                DownloadLinkValidationException
                    or DownloadLinkDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The download link could not be created. See the server log."),
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
