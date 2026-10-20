using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class CustomFieldDependencyException : Xeption
    {
        public CustomFieldDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
