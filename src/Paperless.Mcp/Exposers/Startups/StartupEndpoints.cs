using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;
using Paperless.Mcp.Models.Foundations.UploadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.DownloadLinks;
using Paperless.Mcp.Services.Foundations.UploadLinks;

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

        // Nimmt die Datei zu einem Upload-Link entgegen und reicht sie UNGEPUFFERT an
        // Paperless weiter -- eine 100-MB-PDF liegt dabei nie als Ganzes im Speicher.
        // Akzeptiert multipart/form-data (erster Dateiteil) oder den rohen Rumpf.
        public static async Task<IResult> RedeemUploadLinkAsync(
            string id,
            HttpContext context,
            IUploadLinkService uploadLinkService,
            IDocumentService documentService,
            UploadLinkConfigurations configurations)
        {
            // Das Limit gilt nur hier; der MCP-Endpunkt behaelt das kleine Standardlimit.
            IHttpMaxRequestBodySizeFeature? bodySizeFeature =
                context.Features.Get<IHttpMaxRequestBodySizeFeature>();

            if (bodySizeFeature is { IsReadOnly: false })
            {
                bodySizeFeature.MaxRequestBodySize = configurations.MaxUploadBytes;
            }

            // Vor dem Einloesen: ein zu grosser Versuch verbraucht den Link nicht.
            if (context.Request.ContentLength > configurations.MaxUploadBytes)
            {
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
            }

            UploadLink uploadLink;

            try
            {
                uploadLink = await uploadLinkService.RedeemUploadLinkAsync(id);
            }
            catch (Exception exception) when (
                exception is UploadLinkValidationException
                or UploadLinkDependencyValidationException)
            {
                return Results.NotFound();
            }

            context.Items[PaperlessAuthHandler.RequestTokenItemKey] = uploadLink.Token;

            Stream? uploadedStream = await OpenUploadedFileAsync(context.Request);

            if (uploadedStream is null)
            {
                return Results.BadRequest(
                    "Expected a file: multipart/form-data with a file part, or the raw body.");
            }

            // Zwischenablage auf der Platte statt im Speicher: so kennt die Anfrage an
            // Paperless ihre Laenge. Ein Rumpf ohne Content-Length (chunked) nimmt
            // Django hinter WSGI nicht zuverlaessig an. DeleteOnClose raeumt auf.
            await using var contentStream = new FileStream(
                Path.GetTempFileName(),
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.DeleteOnClose);

            try
            {
                await uploadedStream.CopyToAsync(contentStream, context.RequestAborted);
            }
            catch (BadHttpRequestException exception)
                when (exception.StatusCode == StatusCodes.Status413PayloadTooLarge)
            {
                return Results.StatusCode(StatusCodes.Status413PayloadTooLarge);
            }

            if (contentStream.Length is 0)
            {
                return Results.BadRequest("The uploaded file is empty.");
            }

            contentStream.Position = 0;

            try
            {
                string taskId = await documentService.AddDocumentFileAsync(new DocumentUpload
                {
                    FileName = uploadLink.FileName,
                    ContentStream = contentStream,
                    Title = uploadLink.Title,
                    CorrespondentId = uploadLink.CorrespondentId,
                    DocumentTypeId = uploadLink.DocumentTypeId,
                    StoragePathId = uploadLink.StoragePathId,
                    Created = uploadLink.Created,
                });

                return Results.Ok(new { taskId });
            }
            catch (Exception exception) when (
                exception is DocumentValidationException
                or DocumentDependencyValidationException)
            {
                return Results.BadRequest("Paperless rejected the file.");
            }
            catch (Exception exception) when (
                exception is DocumentDependencyException or DocumentServiceException)
            {
                return Results.StatusCode(StatusCodes.Status502BadGateway);
            }
        }

        private static async Task<Stream?> OpenUploadedFileAsync(HttpRequest request)
        {
            if (MediaTypeHeaderValue.TryParse(request.ContentType, out MediaTypeHeaderValue? mediaType)
                && mediaType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                string boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value ?? string.Empty;

                if (string.IsNullOrWhiteSpace(boundary))
                {
                    return null;
                }

                var reader = new MultipartReader(boundary, request.Body);

                while (await reader.ReadNextSectionAsync() is MultipartSection section)
                {
                    if (ContentDispositionHeaderValue.TryParse(
                            section.ContentDisposition,
                            out ContentDispositionHeaderValue? disposition)
                        && disposition.IsFileDisposition())
                    {
                        return section.Body;
                    }
                }

                return null;
            }

            return request.ContentLength is 0 ? null : request.Body;
        }
    }
}
