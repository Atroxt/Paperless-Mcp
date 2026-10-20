using Xeptions;

namespace Paperless.Mcp.Models.Processings.Correspondents.Exceptions
{
    public class NullCorrespondentProcessingException : Xeption
    {
        public NullCorrespondentProcessingException(string message)
            : base(message)
        { }
    }
}
