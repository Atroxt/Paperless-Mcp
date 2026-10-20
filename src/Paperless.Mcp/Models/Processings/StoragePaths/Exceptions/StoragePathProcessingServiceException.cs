using Xeptions;

namespace Paperless.Mcp.Models.Processings.StoragePaths.Exceptions
{
    public class StoragePathProcessingServiceException : Xeption
    {
        public StoragePathProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
