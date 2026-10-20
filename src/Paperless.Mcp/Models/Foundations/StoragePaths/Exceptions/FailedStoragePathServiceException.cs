using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class FailedStoragePathServiceException : Xeption
    {
        public FailedStoragePathServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
