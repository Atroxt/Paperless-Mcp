using Xeptions;

namespace Paperless.Mcp.Models.Processings.CustomFields.Exceptions
{
    public class CustomFieldProcessingValidationException : Xeption
    {
        public CustomFieldProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
