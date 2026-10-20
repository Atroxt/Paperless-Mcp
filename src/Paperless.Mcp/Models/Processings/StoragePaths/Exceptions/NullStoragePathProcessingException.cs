using Xeptions;

namespace Paperless.Mcp.Models.Processings.StoragePaths.Exceptions
{
    public class NullStoragePathProcessingException : Xeption
    {
        public NullStoragePathProcessingException(string message)
            : base(message)
        { }
    }
}
