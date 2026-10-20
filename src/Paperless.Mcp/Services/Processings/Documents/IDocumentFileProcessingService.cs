namespace Paperless.Mcp.Services.Processings.Documents
{
    // Verbindet die Paperless-Seite mit dem lokalen Dateisystem. Nur im stdio-Transport
    // sinnvoll: dort laeuft der Server auf dem Rechner des Benutzers, "ein Pfad" meint
    // also seinen eigenen.
    public interface IDocumentFileProcessingService
    {
        ValueTask<string> UploadFromPathAsync(
            string filePath,
            string? title,
            int? correspondentId,
            int? documentTypeId,
            int? storagePathId);

        ValueTask<long> DownloadToPathAsync(int documentId, string targetPath, bool original);
    }
}
