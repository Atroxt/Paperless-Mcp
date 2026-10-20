using Xeptions;

namespace Paperless.Mcp.Models.Processings.CustomFields.Exceptions
{
    public class CustomFieldProcessingDependencyException : Xeption
    {
        public CustomFieldProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
