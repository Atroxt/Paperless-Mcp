using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class StoragePathDependencyValidationException : Xeption
    {
        public StoragePathDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
