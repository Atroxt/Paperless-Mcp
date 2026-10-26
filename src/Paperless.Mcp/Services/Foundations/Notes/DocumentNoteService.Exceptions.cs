using Paperless.Mcp.Models.Foundations.Notes.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.Notes
{
    public partial class DocumentNoteService
    {
        // Ein generisches TryCatch statt mehrerer fast gleicher Varianten je Rueckgabetyp:
        // die Zuordnung native Exception -> lokale Exception -> kategoriale Exception
        // steht damit an genau einer Stelle und kann nicht auseinanderlaufen.
        private async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullDocumentNoteException nullDocumentNoteException)
            {
                throw await CreateAndLogValidationExceptionAsync(nullDocumentNoteException);
            }
            catch (InvalidDocumentNoteException invalidDocumentNoteException)
            {
                throw await CreateAndLogValidationExceptionAsync(invalidDocumentNoteException);
            }
            catch (HttpResponseBadRequestException httpResponseBadRequestException)
            {
                var invalidDocumentNoteException =
                    new InvalidDocumentNoteException(
                        message: "Document note is invalid, fix the errors and try again.",
                        innerException: httpResponseBadRequestException);

                throw await CreateAndLogDependencyValidationExceptionAsync(
                    invalidDocumentNoteException);
            }
            // 404 auf eine Ressource heisst "dieses Dokument oder diese Notiz gibt es
            // nicht" und ist damit ein Eingabefehler, kein Infrastrukturproblem. Der
            // falsche Endpunkt kommt getrennt als HttpResponseUrlNotFoundException.
            catch (HttpResponseNotFoundException httpResponseNotFoundException)
            {
                var notFoundDocumentNoteException =
                    new NotFoundDocumentNoteException(
                        message: "Document or note not found.",
                        innerException: httpResponseNotFoundException);

                throw await CreateAndLogDependencyValidationExceptionAsync(
                    notFoundDocumentNoteException);
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
                var failedDocumentNoteServiceException =
                    new FailedDocumentNoteServiceException(
                        message: "Failed document note service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedDocumentNoteServiceException);
            }
        }

        private async ValueTask<DocumentNoteValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var documentNoteValidationException =
                new DocumentNoteValidationException(
                    message: "Document note validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentNoteValidationException);

            return documentNoteValidationException;
        }

        private async ValueTask<DocumentNoteDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedDocumentNoteException =
                new UnauthorizedDocumentNoteException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            return await CreateAndLogDependencyValidationExceptionAsync(
                unauthorizedDocumentNoteException);
        }

        private async ValueTask<DocumentNoteDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var documentNoteDependencyValidationException =
                new DocumentNoteDependencyValidationException(
                    message: "Document note dependency validation error occurred, "
                        + "fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentNoteDependencyValidationException);

            return documentNoteDependencyValidationException;
        }

        private async ValueTask<DocumentNoteDependencyException>
            CreateAndLogCriticalDependencyExceptionAsync(Exception exception)
        {
            var failedDocumentNoteDependencyException =
                new FailedDocumentNoteDependencyException(
                    message: "Failed document note dependency error occurred, contact support.",
                    innerException: exception);

            var documentNoteDependencyException =
                new DocumentNoteDependencyException(
                    message: "Document note dependency error occurred, contact support.",
                    innerException: failedDocumentNoteDependencyException);

            await this.loggingBroker.LogCriticalAsync(documentNoteDependencyException);

            return documentNoteDependencyException;
        }

        private async ValueTask<DocumentNoteDependencyException>
            CreateAndLogDependencyExceptionAsync(Exception exception)
        {
            var failedDocumentNoteDependencyException =
                new FailedDocumentNoteDependencyException(
                    message: "Failed document note dependency error occurred, contact support.",
                    innerException: exception);

            var documentNoteDependencyException =
                new DocumentNoteDependencyException(
                    message: "Document note dependency error occurred, contact support.",
                    innerException: failedDocumentNoteDependencyException);

            await this.loggingBroker.LogErrorAsync(documentNoteDependencyException);

            return documentNoteDependencyException;
        }

        private async ValueTask<DocumentNoteServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var documentNoteServiceException =
                new DocumentNoteServiceException(
                    message: "Document note service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentNoteServiceException);

            return documentNoteServiceException;
        }
    }
}
