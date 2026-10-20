using Xeptions;

namespace Paperless.Mcp.Models.Processings.StoragePaths.Exceptions
{
    public class StoragePathProcessingDependencyException : Xeption
    {
        public StoragePathProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
