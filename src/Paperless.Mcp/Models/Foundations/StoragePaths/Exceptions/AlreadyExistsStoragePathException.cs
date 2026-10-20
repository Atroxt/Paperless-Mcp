using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class AlreadyExistsStoragePathException : Xeption
    {
        public AlreadyExistsStoragePathException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
