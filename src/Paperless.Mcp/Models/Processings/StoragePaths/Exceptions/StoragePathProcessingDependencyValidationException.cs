using Xeptions;

namespace Paperless.Mcp.Models.Processings.StoragePaths.Exceptions
{
    public class StoragePathProcessingDependencyValidationException : Xeption
    {
        public StoragePathProcessingDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
