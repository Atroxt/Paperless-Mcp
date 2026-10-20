using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.Tasks
{
    public partial class TaskService
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
            catch (NullPaperlessTaskException nullPaperlessTaskException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullPaperlessTaskException);
            }
            catch (InvalidPaperlessTaskException invalidPaperlessTaskException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidPaperlessTaskException);
            }
            // Eine unbekannte Kennung ist ein Eingabefehler des Aufrufers.
            catch (NotFoundPaperlessTaskException notFoundPaperlessTaskException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    notFoundPaperlessTaskException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidPaperlessTaskException =
                    new InvalidPaperlessTaskException(
                        message: "PaperlessTask is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidPaperlessTaskException);
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
                var failedPaperlessTaskServiceException =
                    new FailedPaperlessTaskServiceException(
                        message: "Failed paperlessTask service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedPaperlessTaskServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<PaperlessTaskValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var paperlessTaskValidationException =
                new PaperlessTaskValidationException(
                    message: "PaperlessTask validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(paperlessTaskValidationException);

            return paperlessTaskValidationException;
        }

        private async ValueTask<PaperlessTaskDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedPaperlessTaskException =
                new UnauthorizedPaperlessTaskException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedPaperlessTaskException);
        }

        private async ValueTask<PaperlessTaskDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var paperlessTaskDependencyValidationException =
                new PaperlessTaskDependencyValidationException(
                    message: "PaperlessTask dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(paperlessTaskDependencyValidationException);

            return paperlessTaskDependencyValidationException;
        }

        private async ValueTask<PaperlessTaskDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedPaperlessTaskDependencyException =
                new FailedPaperlessTaskDependencyException(
                    message: "Failed paperlessTask dependency error occurred, contact support.",
                    innerException: exception);

            var paperlessTaskDependencyException =
                new PaperlessTaskDependencyException(
                    message: "PaperlessTask dependency error occurred, contact support.",
                    innerException: failedPaperlessTaskDependencyException);

            await this.loggingBroker.LogCriticalAsync(paperlessTaskDependencyException);

            return paperlessTaskDependencyException;
        }

        private async ValueTask<PaperlessTaskDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedPaperlessTaskDependencyException =
                new FailedPaperlessTaskDependencyException(
                    message: "Failed paperlessTask dependency error occurred, contact support.",
                    innerException: exception);

            var paperlessTaskDependencyException =
                new PaperlessTaskDependencyException(
                    message: "PaperlessTask dependency error occurred, contact support.",
                    innerException: failedPaperlessTaskDependencyException);

            await this.loggingBroker.LogErrorAsync(paperlessTaskDependencyException);

            return paperlessTaskDependencyException;
        }

        private async ValueTask<PaperlessTaskServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var paperlessTaskServiceException =
                new PaperlessTaskServiceException(
                    message: "PaperlessTask service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(paperlessTaskServiceException);

            return paperlessTaskServiceException;
        }
    }
}
