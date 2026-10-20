using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    public class AlreadyExistsCorrespondentException : Xeption
    {
        public AlreadyExistsCorrespondentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
