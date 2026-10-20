using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class NotFoundCustomFieldException : Xeption
    {
        public NotFoundCustomFieldException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
