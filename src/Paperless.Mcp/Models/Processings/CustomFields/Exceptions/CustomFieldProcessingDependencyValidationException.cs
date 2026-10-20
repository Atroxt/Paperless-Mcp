using Xeptions;

namespace Paperless.Mcp.Models.Processings.CustomFields.Exceptions
{
    public class CustomFieldProcessingDependencyValidationException : Xeption
    {
        public CustomFieldProcessingDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
