using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.StoragePaths
{
    public partial class StoragePathService
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
            catch (NullStoragePathException nullStoragePathException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullStoragePathException);
            }
            catch (InvalidStoragePathException invalidStoragePathException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidStoragePathException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidStoragePathException =
                    new InvalidStoragePathException(
                        message: "StoragePath is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidStoragePathException);
            }
            catch (HttpResponseConflictException httpResponseConflictException)
            {
                var alreadyExistsStoragePathException =
                    new AlreadyExistsStoragePathException(
                        message: "StoragePath already exists.",
                        innerException: httpResponseConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsStoragePathException);
            }
            // 404 auf eine Ressource heisst "diesen Korrespondenten gibt es nicht" und ist damit ein
            // Eingabefehler, kein Infrastrukturproblem. Der falsche Endpunkt kommt
            // getrennt als HttpResponseUrlNotFoundException.
            catch (HttpResponseNotFoundException httpResponseNotFoundException)
            {
                var notFoundStoragePathException =
                    new NotFoundStoragePathException(
                        message: "StoragePath not found.",
                        innerException: httpResponseNotFoundException);

                throw await CreateAndLogDependencyValidationExceptionAsync(notFoundStoragePathException);
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
                var failedStoragePathServiceException =
                    new FailedStoragePathServiceException(
                        message: "Failed storagePath service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedStoragePathServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<StoragePathValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var storagePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(storagePathValidationException);

            return storagePathValidationException;
        }

        private async ValueTask<StoragePathDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedStoragePathException =
                new UnauthorizedStoragePathException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedStoragePathException);
        }

        private async ValueTask<StoragePathDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var storagePathDependencyValidationException =
                new StoragePathDependencyValidationException(
                    message: "StoragePath dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(storagePathDependencyValidationException);

            return storagePathDependencyValidationException;
        }

        private async ValueTask<StoragePathDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedStoragePathDependencyException =
                new FailedStoragePathDependencyException(
                    message: "Failed storagePath dependency error occurred, contact support.",
                    innerException: exception);

            var storagePathDependencyException =
                new StoragePathDependencyException(
                    message: "StoragePath dependency error occurred, contact support.",
                    innerException: failedStoragePathDependencyException);

            await this.loggingBroker.LogCriticalAsync(storagePathDependencyException);

            return storagePathDependencyException;
        }

        private async ValueTask<StoragePathDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedStoragePathDependencyException =
                new FailedStoragePathDependencyException(
                    message: "Failed storagePath dependency error occurred, contact support.",
                    innerException: exception);

            var storagePathDependencyException =
                new StoragePathDependencyException(
                    message: "StoragePath dependency error occurred, contact support.",
                    innerException: failedStoragePathDependencyException);

            await this.loggingBroker.LogErrorAsync(storagePathDependencyException);

            return storagePathDependencyException;
        }

        private async ValueTask<StoragePathServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var storagePathServiceException =
                new StoragePathServiceException(
                    message: "StoragePath service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(storagePathServiceException);

            return storagePathServiceException;
        }
    }
}
