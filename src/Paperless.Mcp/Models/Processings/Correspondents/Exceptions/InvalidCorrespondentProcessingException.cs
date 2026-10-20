using Xeptions;

namespace Paperless.Mcp.Models.Processings.Correspondents.Exceptions
{
    public class InvalidCorrespondentProcessingException : Xeption
    {
        public InvalidCorrespondentProcessingException(string message)
            : base(message)
        { }
    }
}
