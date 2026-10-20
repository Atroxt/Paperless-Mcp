using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Services.Foundations.StoragePaths
{
    public partial class StoragePathService : IStoragePathService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public StoragePathService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<StoragePath> AddStoragePathAsync(StoragePath storagePath) =>
            TryCatch(async () =>
            {
                ValidateStoragePathOnAdd(storagePath);

                return await this.paperlessBroker.PostStoragePathAsync(storagePath);
            });

        public ValueTask<StoragePath> RetrieveStoragePathByIdAsync(int storagePathId) =>
            TryCatch(async () =>
            {
                ValidateStoragePathId(storagePathId);

                return await this.paperlessBroker.GetStoragePathByIdAsync(storagePathId);
            });

        public ValueTask<StoragePathPage> RetrieveStoragePathPageAsync(int page, int pageSize) =>
            TryCatch(async () =>
            {
                ValidatePageRequest(page, pageSize);

                return await this.paperlessBroker.GetStoragePathPageAsync(page, pageSize);
            });

        public ValueTask<StoragePath> ModifyStoragePathAsync(StoragePath storagePath) =>
            TryCatch(async () =>
            {
                ValidateStoragePathOnModify(storagePath);

                return await this.paperlessBroker.PutStoragePathAsync(storagePath);
            });

        public ValueTask RemoveStoragePathByIdAsync(int storagePathId) =>
            TryCatch(async () =>
            {
                ValidateStoragePathId(storagePathId);

                await this.paperlessBroker.DeleteStoragePathByIdAsync(storagePathId);
            });
    }
}
