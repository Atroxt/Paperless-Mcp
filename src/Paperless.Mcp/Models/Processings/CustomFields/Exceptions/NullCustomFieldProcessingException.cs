using Xeptions;

namespace Paperless.Mcp.Models.Processings.CustomFields.Exceptions
{
    public class NullCustomFieldProcessingException : Xeption
    {
        public NullCustomFieldProcessingException(string message)
            : base(message)
        { }
    }
}
