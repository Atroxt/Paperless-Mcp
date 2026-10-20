using Xeptions;

namespace Paperless.Mcp.Models.Processings.StoragePaths.Exceptions
{
    public class FailedStoragePathProcessingServiceException : Xeption
    {
        public FailedStoragePathProcessingServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
