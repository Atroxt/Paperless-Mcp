using Xeptions;

namespace Paperless.Mcp.Models.Processings.Correspondents.Exceptions
{
    public class CorrespondentProcessingServiceException : Xeption
    {
        public CorrespondentProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
