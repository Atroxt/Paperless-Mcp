using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class CustomFieldServiceException : Xeption
    {
        public CustomFieldServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
