using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class NotFoundStoragePathException : Xeption
    {
        public NotFoundStoragePathException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
