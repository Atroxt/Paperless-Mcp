using Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.GlobalSearches
{
    public partial class GlobalSearchService
    {
        // Ein generisches TryCatch statt dreier fast gleicher Varianten je Rueckgabetyp:
        // die Zuordnung native Exception -> lokale Exception -> kategoriale Exception
        // steht damit an genau einer Stelle und kann nicht auseinanderlaufen.
        private async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullGlobalSearchException nullGlobalSearchException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullGlobalSearchException);
            }
            catch (InvalidGlobalSearchException invalidGlobalSearchException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidGlobalSearchException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidGlobalSearchException =
                    new InvalidGlobalSearchException(
                        message: "GlobalSearch is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidGlobalSearchException);
            }
            // 401 und 403 heissen: das mitgesendete Token traegt nicht. Im HTTP-Transport
            // bringt jeder Aufrufer sein eigenes Token mit, das ist also sein Fehler und
            // von ihm behebbar -- kein Infrastrukturproblem des Betreibers.
            catch (HttpResponseUnauthorizedException httpResponseUnauthorizedException)
            {
                throw await CreateAndLogUnauthorizedExceptionAsync(
                    httpResponseUnauthorizedException);
            }
            catch (HttpResponseForbiddenException httpResponseForbiddenException)
            {
                throw await CreateAndLogUnauthorizedExceptionAsync(
                    httpResponseForbiddenException);
            }
            catch (HttpResponseUrlNotFoundException httpResponseUrlNotFoundException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(
                    httpResponseUrlNotFoundException);
            }
            catch (HttpResponseInternalServerErrorException httpResponseInternalServerErrorException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    httpResponseInternalServerErrorException);
            }
            catch (HttpResponseServiceUnavailableException httpResponseServiceUnavailableException)
            {
                throw await CreateAndLogDependencyExceptionAsync(
                    httpResponseServiceUnavailableException);
            }
            // Jede andere HTTP-Antwort ausserhalb des Erfolgsbereichs: Fehler der
            // Gegenstelle, moeglicherweise voruebergehend.
            catch (HttpResponseException httpResponseException)
            {
                throw await CreateAndLogDependencyExceptionAsync(httpResponseException);
            }
            catch (HttpRequestException httpRequestException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(httpRequestException);
            }
            catch (Exception exception)
            {
                var failedGlobalSearchServiceException =
                    new FailedGlobalSearchServiceException(
                        message: "Failed globalSearch service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedGlobalSearchServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<GlobalSearchValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var globalSearchValidationException =
                new GlobalSearchValidationException(
                    message: "GlobalSearch validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(globalSearchValidationException);

            return globalSearchValidationException;
        }

        private async ValueTask<GlobalSearchDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedGlobalSearchException =
                new UnauthorizedGlobalSearchException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedGlobalSearchException);
        }

        private async ValueTask<GlobalSearchDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var globalSearchDependencyValidationException =
                new GlobalSearchDependencyValidationException(
                    message: "GlobalSearch dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(globalSearchDependencyValidationException);

            return globalSearchDependencyValidationException;
        }

        private async ValueTask<GlobalSearchDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedGlobalSearchDependencyException =
                new FailedGlobalSearchDependencyException(
                    message: "Failed globalSearch dependency error occurred, contact support.",
                    innerException: exception);

            var globalSearchDependencyException =
                new GlobalSearchDependencyException(
                    message: "GlobalSearch dependency error occurred, contact support.",
                    innerException: failedGlobalSearchDependencyException);

            await this.loggingBroker.LogCriticalAsync(globalSearchDependencyException);

            return globalSearchDependencyException;
        }

        private async ValueTask<GlobalSearchDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedGlobalSearchDependencyException =
                new FailedGlobalSearchDependencyException(
                    message: "Failed globalSearch dependency error occurred, contact support.",
                    innerException: exception);

            var globalSearchDependencyException =
                new GlobalSearchDependencyException(
                    message: "GlobalSearch dependency error occurred, contact support.",
                    innerException: failedGlobalSearchDependencyException);

            await this.loggingBroker.LogErrorAsync(globalSearchDependencyException);

            return globalSearchDependencyException;
        }

        private async ValueTask<GlobalSearchServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var globalSearchServiceException =
                new GlobalSearchServiceException(
                    message: "GlobalSearch service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(globalSearchServiceException);

            return globalSearchServiceException;
        }
    }
}
