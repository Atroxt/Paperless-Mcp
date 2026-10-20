using Xeptions;

namespace Paperless.Mcp.Models.Processings.StoragePaths.Exceptions
{
    public class InvalidStoragePathProcessingException : Xeption
    {
        public InvalidStoragePathProcessingException(string message)
            : base(message)
        { }
    }
}
