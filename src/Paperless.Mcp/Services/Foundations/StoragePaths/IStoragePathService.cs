using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Services.Foundations.StoragePaths
{
    // Foundation Service: uebersetzt Technologiesprache in Geschaeftssprache
    // (Post -> Add, Get -> Retrieve, Put -> Modify, Delete -> Remove) und ist die
    // letzte Abstraktionsschicht vor der eigentlichen Geschaeftslogik.
    public interface IStoragePathService
    {
        ValueTask<StoragePath> AddStoragePathAsync(StoragePath storagePath);
        ValueTask<StoragePath> RetrieveStoragePathByIdAsync(int storagePathId);
        ValueTask<StoragePathPage> RetrieveStoragePathPageAsync(int page, int pageSize);
        ValueTask<StoragePath> ModifyStoragePathAsync(StoragePath storagePath);
        ValueTask RemoveStoragePathByIdAsync(int storagePathId);
    }
}
