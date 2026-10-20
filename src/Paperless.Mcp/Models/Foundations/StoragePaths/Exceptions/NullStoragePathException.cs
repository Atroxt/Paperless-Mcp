using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    public class NullStoragePathException : Xeption
    {
        public NullStoragePathException(string message)
            : base(message)
        { }
    }
}
