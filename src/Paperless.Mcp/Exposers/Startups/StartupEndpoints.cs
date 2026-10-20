using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.DownloadLinks;

namespace Paperless.Mcp.Exposers.Startups
{
    internal static class StartupEndpoints
    {
        public static async Task RequireRequestTokenAsync(HttpContext context, Func<Task> next)
        {
            if (context.Request.Path.StartsWithSegments("/mcp"))
            {
                string? requestToken = PaperlessAuthHandler.ReadRequestToken(context.Request);

                if (string.IsNullOrWhiteSpace(requestToken))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.Headers.WWWAuthenticate = "Bearer";

                    return;
                }
            }

            await next().ConfigureAwait(false);
        }

        public static async Task<IResult> RedeemDownloadLinkAsync(
            string id,
            HttpContext context,
            IDownloadLinkService downloadLinkService,
            IDocumentService documentService)
        {
            DownloadLink downloadLink;

            try
            {
                downloadLink = await downloadLinkService.RedeemDownloadLinkAsync(id);
            }
            catch (Exception exception) when (
                exception is DownloadLinkValidationException
                or DownloadLinkDependencyValidationException)
            {
                // Unbekannt, verbraucht, abgelaufen oder unsinnig -- von aussen ist das
                // ununterscheidbar, und genau so soll es bleiben.
                return Results.NotFound();
            }

            // Der PaperlessAuthHandler liest das Token aus HttpContext.Items: diese Anfrage
            // hat keinen Authorization-Header, wohl aber den beim Ausstellen hinterlegten
            // Ausweis. Der Download laeuft damit mit den Rechten des Aufrufers.
            context.Items[PaperlessAuthHandler.RequestTokenItemKey] = downloadLink.Token;

            DocumentFile documentFile;

            try
            {
                documentFile = await documentService.RetrieveDocumentFileWithMetadataAsync(
                    downloadLink.DocumentId, downloadLink.Original);
            }
            catch (Exception exception) when (
                exception is DocumentValidationException
                or DocumentDependencyValidationException)
            {
                return Results.NotFound();
            }

            // Kein Zwischenspeichern: die Datei ist vertraulich, und der Link galt nur
            // dieses eine Mal.
            context.Response.Headers.CacheControl = "no-store";

            return Results.File(
                documentFile.Content,
                contentType: documentFile.ContentType,
                fileDownloadName: string.IsNullOrWhiteSpace(documentFile.FileName)
                    ? $"document-{downloadLink.DocumentId}"
                    : documentFile.FileName);
        }
    }
}
