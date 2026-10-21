using Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.UploadLinks
{
    public partial class UploadLinkService
    {
        // Diese Service spricht keine fremde API an, sondern nur den eigenen Vorrat.
        // Entsprechend gibt es keine HTTP-Faelle, nur Eingabefehler und Unerwartetes.
        private async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullUploadLinkException nullUploadLinkException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullUploadLinkException);
            }
            catch (InvalidUploadLinkException invalidUploadLinkException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidUploadLinkException);
            }
            // Ein unbekannter, verbrauchter oder abgelaufener Link ist ein Eingabefehler
            // des Aufrufers, kein Ausfall.
            catch (NotFoundUploadLinkException notFoundUploadLinkException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    notFoundUploadLinkException);
            }
            catch (Exception exception)
            {
                var failedUploadLinkServiceException =
                    new FailedUploadLinkServiceException(
                        message: "Failed upload link service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedUploadLinkServiceException);
            }
        }

        private async ValueTask<UploadLinkValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var uploadLinkValidationException =
                new UploadLinkValidationException(
                    message: "UploadLink validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(uploadLinkValidationException);

            return uploadLinkValidationException;
        }

        private async ValueTask<UploadLinkDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var uploadLinkDependencyValidationException =
                new UploadLinkDependencyValidationException(
                    message: "UploadLink dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(uploadLinkDependencyValidationException);

            return uploadLinkDependencyValidationException;
        }

        private async ValueTask<UploadLinkServiceException>
            CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var uploadLinkServiceException =
                new UploadLinkServiceException(
                    message: "UploadLink service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(uploadLinkServiceException);

            return uploadLinkServiceException;
        }
    }
}
