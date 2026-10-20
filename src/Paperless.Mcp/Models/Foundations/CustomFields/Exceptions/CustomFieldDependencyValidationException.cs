using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class CustomFieldDependencyValidationException : Xeption
    {
        public CustomFieldDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
