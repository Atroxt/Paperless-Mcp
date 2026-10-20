using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeService
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
            catch (NullDocumentTypeException nullDocumentTypeException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullDocumentTypeException);
            }
            catch (InvalidDocumentTypeException invalidDocumentTypeException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidDocumentTypeException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidDocumentTypeException =
                    new InvalidDocumentTypeException(
                        message: "DocumentType is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidDocumentTypeException);
            }
            catch (HttpResponseConflictException httpResponseConflictException)
            {
                var alreadyExistsDocumentTypeException =
                    new AlreadyExistsDocumentTypeException(
                        message: "DocumentType already exists.",
                        innerException: httpResponseConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsDocumentTypeException);
            }
            // 404 auf eine Ressource heisst "diesen Korrespondenten gibt es nicht" und ist damit ein
            // Eingabefehler, kein Infrastrukturproblem. Der falsche Endpunkt kommt
            // getrennt als HttpResponseUrlNotFoundException.
            catch (HttpResponseNotFoundException httpResponseNotFoundException)
            {
                var notFoundDocumentTypeException =
                    new NotFoundDocumentTypeException(
                        message: "DocumentType not found.",
                        innerException: httpResponseNotFoundException);

                throw await CreateAndLogDependencyValidationExceptionAsync(notFoundDocumentTypeException);
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
                var failedDocumentTypeServiceException =
                    new FailedDocumentTypeServiceException(
                        message: "Failed documentType service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedDocumentTypeServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<DocumentTypeValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var documentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentTypeValidationException);

            return documentTypeValidationException;
        }

        private async ValueTask<DocumentTypeDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedDocumentTypeException =
                new UnauthorizedDocumentTypeException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedDocumentTypeException);
        }

        private async ValueTask<DocumentTypeDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var documentTypeDependencyValidationException =
                new DocumentTypeDependencyValidationException(
                    message: "DocumentType dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentTypeDependencyValidationException);

            return documentTypeDependencyValidationException;
        }

        private async ValueTask<DocumentTypeDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedDocumentTypeDependencyException =
                new FailedDocumentTypeDependencyException(
                    message: "Failed documentType dependency error occurred, contact support.",
                    innerException: exception);

            var documentTypeDependencyException =
                new DocumentTypeDependencyException(
                    message: "DocumentType dependency error occurred, contact support.",
                    innerException: failedDocumentTypeDependencyException);

            await this.loggingBroker.LogCriticalAsync(documentTypeDependencyException);

            return documentTypeDependencyException;
        }

        private async ValueTask<DocumentTypeDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedDocumentTypeDependencyException =
                new FailedDocumentTypeDependencyException(
                    message: "Failed documentType dependency error occurred, contact support.",
                    innerException: exception);

            var documentTypeDependencyException =
                new DocumentTypeDependencyException(
                    message: "DocumentType dependency error occurred, contact support.",
                    innerException: failedDocumentTypeDependencyException);

            await this.loggingBroker.LogErrorAsync(documentTypeDependencyException);

            return documentTypeDependencyException;
        }

        private async ValueTask<DocumentTypeServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var documentTypeServiceException =
                new DocumentTypeServiceException(
                    message: "DocumentType service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentTypeServiceException);

            return documentTypeServiceException;
        }
    }
}
