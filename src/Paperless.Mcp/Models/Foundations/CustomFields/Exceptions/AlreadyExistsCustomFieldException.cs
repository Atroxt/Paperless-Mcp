using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class AlreadyExistsCustomFieldException : Xeption
    {
        public AlreadyExistsCustomFieldException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
