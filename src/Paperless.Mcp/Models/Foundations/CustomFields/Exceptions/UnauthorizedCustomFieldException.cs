using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class UnauthorizedCustomFieldException : Xeption
    {
        public UnauthorizedCustomFieldException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
