using Xeptions;

namespace Paperless.Mcp.Models.Processings.CustomFields.Exceptions
{
    public class CustomFieldProcessingServiceException : Xeption
    {
        public CustomFieldProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
