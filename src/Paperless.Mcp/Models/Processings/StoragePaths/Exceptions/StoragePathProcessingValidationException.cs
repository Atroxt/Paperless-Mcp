using Xeptions;

namespace Paperless.Mcp.Models.Processings.StoragePaths.Exceptions
{
    public class StoragePathProcessingValidationException : Xeption
    {
        public StoragePathProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
