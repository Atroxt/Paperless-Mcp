using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.CustomFields
{
    public partial class CustomFieldService
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
            catch (NullCustomFieldException nullCustomFieldException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullCustomFieldException);
            }
            catch (InvalidCustomFieldException invalidCustomFieldException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidCustomFieldException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidCustomFieldException =
                    new InvalidCustomFieldException(
                        message: "CustomField is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidCustomFieldException);
            }
            catch (HttpResponseConflictException httpResponseConflictException)
            {
                var alreadyExistsCustomFieldException =
                    new AlreadyExistsCustomFieldException(
                        message: "CustomField already exists.",
                        innerException: httpResponseConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsCustomFieldException);
            }
            // 404 auf eine Ressource heisst "diesen Korrespondenten gibt es nicht" und ist damit ein
            // Eingabefehler, kein Infrastrukturproblem. Der falsche Endpunkt kommt
            // getrennt als HttpResponseUrlNotFoundException.
            catch (HttpResponseNotFoundException httpResponseNotFoundException)
            {
                var notFoundCustomFieldException =
                    new NotFoundCustomFieldException(
                        message: "CustomField not found.",
                        innerException: httpResponseNotFoundException);

                throw await CreateAndLogDependencyValidationExceptionAsync(notFoundCustomFieldException);
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
                var failedCustomFieldServiceException =
                    new FailedCustomFieldServiceException(
                        message: "Failed customField service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedCustomFieldServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<CustomFieldValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var customFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(customFieldValidationException);

            return customFieldValidationException;
        }

        private async ValueTask<CustomFieldDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedCustomFieldException =
                new UnauthorizedCustomFieldException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedCustomFieldException);
        }

        private async ValueTask<CustomFieldDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var customFieldDependencyValidationException =
                new CustomFieldDependencyValidationException(
                    message: "CustomField dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(customFieldDependencyValidationException);

            return customFieldDependencyValidationException;
        }

        private async ValueTask<CustomFieldDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedCustomFieldDependencyException =
                new FailedCustomFieldDependencyException(
                    message: "Failed customField dependency error occurred, contact support.",
                    innerException: exception);

            var customFieldDependencyException =
                new CustomFieldDependencyException(
                    message: "CustomField dependency error occurred, contact support.",
                    innerException: failedCustomFieldDependencyException);

            await this.loggingBroker.LogCriticalAsync(customFieldDependencyException);

            return customFieldDependencyException;
        }

        private async ValueTask<CustomFieldDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedCustomFieldDependencyException =
                new FailedCustomFieldDependencyException(
                    message: "Failed customField dependency error occurred, contact support.",
                    innerException: exception);

            var customFieldDependencyException =
                new CustomFieldDependencyException(
                    message: "CustomField dependency error occurred, contact support.",
                    innerException: failedCustomFieldDependencyException);

            await this.loggingBroker.LogErrorAsync(customFieldDependencyException);

            return customFieldDependencyException;
        }

        private async ValueTask<CustomFieldServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var customFieldServiceException =
                new CustomFieldServiceException(
                    message: "CustomField service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(customFieldServiceException);

            return customFieldServiceException;
        }
    }
}
