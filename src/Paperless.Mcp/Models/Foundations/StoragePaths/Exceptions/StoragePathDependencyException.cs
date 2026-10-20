using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class StoragePathDependencyException : Xeption
    {
        public StoragePathDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
