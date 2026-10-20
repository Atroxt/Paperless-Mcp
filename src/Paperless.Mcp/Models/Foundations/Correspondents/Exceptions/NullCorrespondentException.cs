using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class NullCorrespondentException : Xeption
    {
        public NullCorrespondentException(string message)
            : base(message)
        { }
    }
}
