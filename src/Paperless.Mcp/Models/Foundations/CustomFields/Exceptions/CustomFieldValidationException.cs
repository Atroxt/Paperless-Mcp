using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class CustomFieldValidationException : Xeption
    {
        public CustomFieldValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
