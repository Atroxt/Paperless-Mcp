using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class StoragePathServiceException : Xeption
    {
        public StoragePathServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
