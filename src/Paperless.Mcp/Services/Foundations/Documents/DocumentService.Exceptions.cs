using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.Documents
{
    public partial class DocumentService
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
            catch (NullDocumentException nullDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullDocumentException);
            }
            catch (InvalidDocumentException invalidDocumentException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidDocumentException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidDocumentException =
                    new InvalidDocumentException(
                        message: "Document is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(invalidDocumentException);
            }
            catch (HttpResponseConflictException httpResponseConflictException)
            {
                var alreadyExistsDocumentException =
                    new AlreadyExistsDocumentException(
                        message: "Document already exists.",
                        innerException: httpResponseConflictException);

                throw await CreateAndLogDependencyValidationExceptionAsync(alreadyExistsDocumentException);
            }
            // 404 auf eine Ressource heisst "diesen Korrespondenten gibt es nicht" und ist damit ein
            // Eingabefehler, kein Infrastrukturproblem. Der falsche Endpunkt kommt
            // getrennt als HttpResponseUrlNotFoundException.
            catch (HttpResponseNotFoundException httpResponseNotFoundException)
            {
                var notFoundDocumentException =
                    new NotFoundDocumentException(
                        message: "Document not found.",
                        innerException: httpResponseNotFoundException);

                throw await CreateAndLogDependencyValidationExceptionAsync(notFoundDocumentException);
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
                var failedDocumentServiceException =
                    new FailedDocumentServiceException(
                        message: "Failed document service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedDocumentServiceException);
            }
        }

        private async ValueTask TryCatch(Func<ValueTask> returningNothingFunction) =>
            await TryCatch(async () =>
            {
                await returningNothingFunction();

                return default(object);
            });

        private async ValueTask<DocumentValidationException> CreateAndLogValidationExceptionAsync(
            Xeption exception)
        {
            var documentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentValidationException);

            return documentValidationException;
        }

        private async ValueTask<DocumentDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedDocumentException =
                new UnauthorizedDocumentException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedDocumentException);
        }

        private async ValueTask<DocumentDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var documentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentDependencyValidationException);

            return documentDependencyValidationException;
        }

        private async ValueTask<DocumentDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: exception);

            var documentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            await this.loggingBroker.LogCriticalAsync(documentDependencyException);

            return documentDependencyException;
        }

        private async ValueTask<DocumentDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: exception);

            var documentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            await this.loggingBroker.LogErrorAsync(documentDependencyException);

            return documentDependencyException;
        }

        private async ValueTask<DocumentServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var documentServiceException =
                new DocumentServiceException(
                    message: "Document service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentServiceException);

            return documentServiceException;
        }
    }
}
