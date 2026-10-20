using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string StoragePathsRelativeUrl = "api/storage_paths/";

        public async ValueTask<StoragePath> PostStoragePathAsync(StoragePath storagePath) =>
            await PostAsync(StoragePathsRelativeUrl, storagePath);

        public async ValueTask<StoragePath> GetStoragePathByIdAsync(int storagePathId) =>
            await GetAsync<StoragePath>($"{StoragePathsRelativeUrl}{storagePathId}/");

        public async ValueTask<StoragePathPage> GetStoragePathPageAsync(int page, int pageSize) =>
            await GetAsync<StoragePathPage>(
                $"{StoragePathsRelativeUrl}?page={page}&page_size={pageSize}");

        public async ValueTask<StoragePath> PutStoragePathAsync(StoragePath storagePath) =>
            await PutAsync($"{StoragePathsRelativeUrl}{storagePath.Id}/", storagePath);

        public async ValueTask DeleteStoragePathByIdAsync(int storagePathId) =>
            await DeleteAsync($"{StoragePathsRelativeUrl}{storagePathId}/");
    }
}
