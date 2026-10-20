using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Processings.StoragePaths;
using Paperless.Mcp.Services.Foundations.StoragePaths;

namespace Paperless.Mcp.Services.Processings.StoragePaths
{
    // Hoeherwertige Logik einer Entity: holt erst den StoragePath, um die Tragweite zu kennen,
    // und entfernt ihn nur bei ausdruecklicher Bestaetigung. Zwei Schritte auf einer
    // Foundation Service -- genau das, wofuer die Processing-Schicht da ist.
    public partial class StoragePathProcessingService : IStoragePathProcessingService
    {
        private readonly IStoragePathService storagePathService;

        public StoragePathProcessingService(IStoragePathService storagePathService) =>
            this.storagePathService = storagePathService;

        public ValueTask<StoragePathRemovalPlan> RemoveStoragePathByIdAsync(int storagePathId, bool confirm) =>
            TryCatch(async () =>
            {
                ValidateStoragePathIdOnRemove(storagePathId);

                // Immer zuerst lesen: der Bericht soll die Tragweite nennen, und zwar
                // auch dann, wenn anschliessend geloescht wird.
                StoragePath storagePath = await this.storagePathService.RetrieveStoragePathByIdAsync(storagePathId);

                if (confirm is false)
                {
                    return CreateDryRunPlan(storagePath);
                }

                await this.storagePathService.RemoveStoragePathByIdAsync(storagePathId);

                return CreateRemovedPlan(storagePath);
            });

        private static StoragePathRemovalPlan CreateDryRunPlan(StoragePath storagePath) =>
            new()
            {
                StoragePathId = storagePath.Id,
                Name = storagePath.Name,
                DocumentCount = storagePath.DocumentCount,
                Removed = false,

                Summary =
                    $"Dry run: storagePath '{storagePath.Name}' would be deleted. "
                    + $"{DescribeDocumentCount(storagePath.DocumentCount)} "
                    + "Call again with confirm set to true to actually delete it.",
            };

        private static StoragePathRemovalPlan CreateRemovedPlan(StoragePath storagePath) =>
            new()
            {
                StoragePathId = storagePath.Id,
                Name = storagePath.Name,
                DocumentCount = storagePath.DocumentCount,
                Removed = true,

                Summary =
                    $"StoragePath '{storagePath.Name}' was deleted. "
                    + $"{DescribeDocumentCount(storagePath.DocumentCount)}",
            };

        private static string DescribeDocumentCount(int documentCount) =>
            documentCount is 0
                ? "No document uses it."
                : $"{documentCount} document(s) use it and fall back to the default "
                  + "location; the documents themselves are kept.";
    }
}
