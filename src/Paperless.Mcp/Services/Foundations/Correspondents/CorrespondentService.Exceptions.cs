using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.Correspondents
{
    public partial class CorrespondentService
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
            catch (NullCorrespondentException nullCorrespondentException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullCorrespondentException);
            }
            catch (InvalidCorrespondentException invalidCorrespondentException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidCorrespondentException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidCorrespondentException =
                    new InvalidCorrespondentException(
                        message: "Correspondent is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidCorrespondentException);
            }
            catch (HttpResponseConflictException httpResponseConflictException)
            {
                var alreadyExistsCorrespondentException =
                    new AlreadyExistsCorrespondentException(
                        message: "Correspondent already exists.",
                        innerException: httpResponseConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsCorrespondentException);
            }
            // 404 auf eine Ressource heisst "diesen Korrespondenten gibt es nicht" und ist damit ein
            // Eingabefehler, kein Infrastrukturproblem. Der falsche Endpunkt kommt
            // getrennt als HttpResponseUrlNotFoundException.
            catch (HttpResponseNotFoundException httpResponseNotFoundException)
            {
                var notFoundCorrespondentException =
                    new NotFoundCorrespondentException(
                        message: "Correspondent not found.",
                        innerException: httpResponseNotFoundException);

                throw await CreateAndLogDependencyValidationExceptionAsync(notFoundCorrespondentException);
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
                var failedCorrespondentServiceException =
                    new FailedCorrespondentServiceException(
                        message: "Failed correspondent service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedCorrespondentServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<CorrespondentValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var correspondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(correspondentValidationException);

            return correspondentValidationException;
        }

        private async ValueTask<CorrespondentDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedCorrespondentException =
                new UnauthorizedCorrespondentException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedCorrespondentException);
        }

        private async ValueTask<CorrespondentDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var correspondentDependencyValidationException =
                new CorrespondentDependencyValidationException(
                    message: "Correspondent dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(correspondentDependencyValidationException);

            return correspondentDependencyValidationException;
        }

        private async ValueTask<CorrespondentDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedCorrespondentDependencyException =
                new FailedCorrespondentDependencyException(
                    message: "Failed correspondent dependency error occurred, contact support.",
                    innerException: exception);

            var correspondentDependencyException =
                new CorrespondentDependencyException(
                    message: "Correspondent dependency error occurred, contact support.",
                    innerException: failedCorrespondentDependencyException);

            await this.loggingBroker.LogCriticalAsync(correspondentDependencyException);

            return correspondentDependencyException;
        }

        private async ValueTask<CorrespondentDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedCorrespondentDependencyException =
                new FailedCorrespondentDependencyException(
                    message: "Failed correspondent dependency error occurred, contact support.",
                    innerException: exception);

            var correspondentDependencyException =
                new CorrespondentDependencyException(
                    message: "Correspondent dependency error occurred, contact support.",
                    innerException: failedCorrespondentDependencyException);

            await this.loggingBroker.LogErrorAsync(correspondentDependencyException);

            return correspondentDependencyException;
        }

        private async ValueTask<CorrespondentServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var correspondentServiceException =
                new CorrespondentServiceException(
                    message: "Correspondent service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(correspondentServiceException);

            return correspondentServiceException;
        }
    }
}
