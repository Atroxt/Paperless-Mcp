using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class FailedCustomFieldDependencyException : Xeption
    {
        public FailedCustomFieldDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
