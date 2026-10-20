using Xeptions;

namespace Paperless.Mcp.Models.Processings.CustomFields.Exceptions
{
    public class InvalidCustomFieldProcessingException : Xeption
    {
        public InvalidCustomFieldProcessingException(string message)
            : base(message)
        { }
    }
}
