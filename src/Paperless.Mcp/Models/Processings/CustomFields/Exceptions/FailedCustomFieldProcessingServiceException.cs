using Xeptions;

namespace Paperless.Mcp.Models.Processings.CustomFields.Exceptions
{
    public class FailedCustomFieldProcessingServiceException : Xeption
    {
        public FailedCustomFieldProcessingServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
