using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class FailedCustomFieldServiceException : Xeption
    {
        public FailedCustomFieldServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
