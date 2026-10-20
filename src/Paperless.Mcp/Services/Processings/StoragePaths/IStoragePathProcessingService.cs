using Paperless.Mcp.Models.Processings.StoragePaths;

namespace Paperless.Mcp.Services.Processings.StoragePaths
{
    public interface IStoragePathProcessingService
    {
        // confirm entscheidet, ob wirklich geloescht wird. false ist der Default und
        // berichtet nur, was geschehen wuerde.
        ValueTask<StoragePathRemovalPlan> RemoveStoragePathByIdAsync(int storagePathId, bool confirm);
    }
}
