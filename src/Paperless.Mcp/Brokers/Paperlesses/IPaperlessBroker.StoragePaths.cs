using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<StoragePath> PostStoragePathAsync(StoragePath storagePath);
        ValueTask<StoragePath> GetStoragePathByIdAsync(int storagePathId);
        ValueTask<StoragePathPage> GetStoragePathPageAsync(int page, int pageSize);
        ValueTask<StoragePath> PutStoragePathAsync(StoragePath storagePath);
        ValueTask DeleteStoragePathByIdAsync(int storagePathId);
    }
}
