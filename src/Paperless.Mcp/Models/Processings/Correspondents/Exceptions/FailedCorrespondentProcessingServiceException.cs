using Xeptions;

namespace Paperless.Mcp.Models.Processings.Correspondents.Exceptions
{
    public class FailedCorrespondentProcessingServiceException : Xeption
    {
        public FailedCorrespondentProcessingServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
