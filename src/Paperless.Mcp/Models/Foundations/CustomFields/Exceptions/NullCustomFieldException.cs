using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    public class NullCustomFieldException : Xeption
    {
        public NullCustomFieldException(string message)
            : base(message)
        { }
    }
}
