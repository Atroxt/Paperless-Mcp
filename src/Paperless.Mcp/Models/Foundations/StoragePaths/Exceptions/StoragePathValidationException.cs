using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class StoragePathValidationException : Xeption
    {
        public StoragePathValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
