using Paperless.Mcp.Models.Processings.Healths.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.Healths
{
    public partial class HealthProcessingService
    {
        // Erwartbare Abhaengigkeitsfehler werden oben zu Feldern des Berichts. Hier
        // bleibt nur, was wirklich unerwartet ist.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (Exception exception)
            {
                var failedHealthProcessingServiceException =
                    new FailedHealthProcessingServiceException(
                        message: "Failed health processing service error occurred, contact support.",
                        innerException: exception);

                throw new HealthProcessingServiceException(
                    message: "Health processing service error occurred, contact support.",
                    innerException: failedHealthProcessingServiceException);
            }
        }
    }
}
