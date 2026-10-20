using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class NullDocumentTypeException : Xeption
    {
        public NullDocumentTypeException(string message)
            : base(message)
        { }
    }
}
