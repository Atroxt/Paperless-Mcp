using Xeptions;

namespace Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidStoragePathException : Xeption
    {
        public InvalidStoragePathException(string message)
            : base(message)
        { }

        public InvalidStoragePathException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
