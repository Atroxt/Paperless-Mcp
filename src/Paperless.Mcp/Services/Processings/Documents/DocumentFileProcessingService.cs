using Paperless.Mcp.Brokers.Files;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;

namespace Paperless.Mcp.Services.Processings.Documents
{
    // Genau eine Foundation Service plus einen Support-Broker fuer das Dateisystem --
    // mehr darf eine Processing Service nicht kennen.
    public partial class DocumentFileProcessingService : IDocumentFileProcessingService
    {
        private readonly IDocumentService documentService;
        private readonly IFileBroker fileBroker;
        private readonly DocumentFileConfigurations configurations;

        public DocumentFileProcessingService(
            IDocumentService documentService,
            IFileBroker fileBroker,
            DocumentFileConfigurations configurations)
        {
            this.documentService = documentService;
            this.fileBroker = fileBroker;
            this.configurations = configurations;
        }

        public ValueTask<string> UploadFromPathAsync(
            string filePath,
            string? title,
            int? correspondentId,
            int? documentTypeId,
            int? storagePathId) =>
            TryCatch(async () =>
            {
                GuardLocalFileAccess();
                ValidateFilePath(filePath);

                if (await this.fileBroker.ExistsAsync(filePath) is false)
                {
                    throw new NotFoundDocumentFileException(
                        message: $"No file found at '{filePath}'.");
                }

                // Gestreamt statt ganz gelesen: grosse PDFs liegen so nie komplett im
                // Arbeitsspeicher.
                await using Stream contentStream = await this.fileBroker.OpenReadAsync(filePath);

                return await this.documentService.AddDocumentFileAsync(new DocumentUpload
                {
                    FileName = Path.GetFileName(filePath),
                    ContentStream = contentStream,
                    Title = title,
                    CorrespondentId = correspondentId,
                    DocumentTypeId = documentTypeId,
                    StoragePathId = storagePathId,
                });
            });

        public ValueTask<long> DownloadToPathAsync(
            int documentId,
            string targetPath,
            bool original) =>
            TryCatch(async () =>
            {
                GuardLocalFileAccess();
                ValidateDocumentIdAndTargetPath(documentId, targetPath);

                byte[] content =
                    await this.documentService.RetrieveDocumentFileAsync(documentId, original);

                await this.fileBroker.WriteAllBytesAsync(targetPath, content);

                return (long)content.Length;
            });

        // Zweite Verteidigungslinie. Die erste ist Program.cs, das diese Werkzeuge im
        // HTTP-Transport gar nicht erst anmeldet -- aber der Schutz darf nicht an einer
        // einzigen Stelle haengen.
        private void GuardLocalFileAccess()
        {
            if (this.configurations.AllowLocalFileAccess is false)
            {
                throw new ForbiddenDocumentFileException(
                    message:
                        "Local file access is disabled. These tools read and write on the "
                        + "machine the server runs on, which is only meaningful over the "
                        + "stdio transport.");
            }
        }
    }
}
