using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class FailedStoragePathDependencyException : Xeption
    {
        public FailedStoragePathDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
