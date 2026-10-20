using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class UnauthorizedStoragePathException : Xeption
    {
        public UnauthorizedStoragePathException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
