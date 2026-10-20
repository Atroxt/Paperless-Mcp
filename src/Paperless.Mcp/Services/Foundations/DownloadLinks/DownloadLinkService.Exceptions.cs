using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.DownloadLinks
{
    public partial class DownloadLinkService
    {
        // Diese Service spricht keine fremde API an, sondern nur den eigenen Vorrat.
        // Entsprechend gibt es keine HTTP-Faelle, nur Eingabefehler und Unerwartetes.
        private async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullDownloadLinkException nullDownloadLinkException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullDownloadLinkException);
            }
            catch (InvalidDownloadLinkException invalidDownloadLinkException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidDownloadLinkException);
            }
            // Ein unbekannter, verbrauchter oder abgelaufener Link ist ein Eingabefehler
            // des Aufrufers, kein Ausfall.
            catch (NotFoundDownloadLinkException notFoundDownloadLinkException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    notFoundDownloadLinkException);
            }
            catch (Exception exception)
            {
                var failedDownloadLinkServiceException =
                    new FailedDownloadLinkServiceException(
                        message: "Failed download link service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedDownloadLinkServiceException);
            }
        }

        private async ValueTask<DownloadLinkValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var downloadLinkValidationException =
                new DownloadLinkValidationException(
                    message: "DownloadLink validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(downloadLinkValidationException);

            return downloadLinkValidationException;
        }

        private async ValueTask<DownloadLinkDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var downloadLinkDependencyValidationException =
                new DownloadLinkDependencyValidationException(
                    message: "DownloadLink dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(downloadLinkDependencyValidationException);

            return downloadLinkDependencyValidationException;
        }

        private async ValueTask<DownloadLinkServiceException>
            CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var downloadLinkServiceException =
                new DownloadLinkServiceException(
                    message: "DownloadLink service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(downloadLinkServiceException);

            return downloadLinkServiceException;
        }
    }
}
