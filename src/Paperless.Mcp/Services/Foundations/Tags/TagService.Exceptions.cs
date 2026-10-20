using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.Tags
{
    public partial class TagService
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
            catch (NullTagException nullTagException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullTagException);
            }
            catch (InvalidTagException invalidTagException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidTagException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidTagException =
                    new InvalidTagException(
                        message: "Tag is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidTagException);
            }
            catch (HttpResponseConflictException httpResponseConflictException)
            {
                var alreadyExistsTagException =
                    new AlreadyExistsTagException(
                        message: "Tag already exists.",
                        innerException: httpResponseConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsTagException);
            }
            // 404 auf eine Ressource heisst "diesen Tag gibt es nicht" und ist damit ein
            // Eingabefehler, kein Infrastrukturproblem. Der falsche Endpunkt kommt
            // getrennt als HttpResponseUrlNotFoundException.
            catch (HttpResponseNotFoundException httpResponseNotFoundException)
            {
                var notFoundTagException =
                    new NotFoundTagException(
                        message: "Tag not found.",
                        innerException: httpResponseNotFoundException);

                throw await CreateAndLogDependencyValidationExceptionAsync(notFoundTagException);
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
                var failedTagServiceException =
                    new FailedTagServiceException(
                        message: "Failed tag service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedTagServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<TagValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var tagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(tagValidationException);

            return tagValidationException;
        }

        private async ValueTask<TagDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedTagException =
                new UnauthorizedTagException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedTagException);
        }

        private async ValueTask<TagDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var tagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(tagDependencyValidationException);

            return tagDependencyValidationException;
        }

        private async ValueTask<TagDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: exception);

            var tagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            await this.loggingBroker.LogCriticalAsync(tagDependencyException);

            return tagDependencyException;
        }

        private async ValueTask<TagDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: exception);

            var tagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            await this.loggingBroker.LogErrorAsync(tagDependencyException);

            return tagDependencyException;
        }

        private async ValueTask<TagServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var tagServiceException =
                new TagServiceException(
                    message: "Tag service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(tagServiceException);

            return tagServiceException;
        }
    }
}
