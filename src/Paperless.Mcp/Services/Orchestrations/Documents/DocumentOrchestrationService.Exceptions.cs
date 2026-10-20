using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;
using Paperless.Mcp.Models.Orchestrations.Documents.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Orchestrations.Documents
{
    public partial class DocumentOrchestrationService
    {
        // Zwei Entities, also zwei Exception-Familien -- beide werden hier in die
        // eigene Kategorie uebersetzt. Der Aufrufer soll nicht wissen muessen, ob der
        // Fehler vom Dokument oder vom Auftrag kam.
        private async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullDocumentOrchestrationException nullDocumentOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(
                    nullDocumentOrchestrationException);
            }
            catch (InvalidDocumentOrchestrationException invalidDocumentOrchestrationException)
            {
                throw await CreateAndLogValidationExceptionAsync(
                    invalidDocumentOrchestrationException);
            }
            catch (DocumentValidationException documentValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    documentValidationException);
            }
            catch (DocumentDependencyValidationException documentDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    documentDependencyValidationException);
            }
            catch (PaperlessTaskValidationException paperlessTaskValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    paperlessTaskValidationException);
            }
            catch (PaperlessTaskDependencyValidationException paperlessTaskDependencyValidationException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    paperlessTaskDependencyValidationException);
            }
            catch (DocumentDependencyException documentDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(documentDependencyException);
            }
            catch (PaperlessTaskDependencyException paperlessTaskDependencyException)
            {
                throw await CreateAndLogDependencyExceptionAsync(paperlessTaskDependencyException);
            }
            catch (DocumentServiceException documentServiceException)
            {
                throw await CreateAndLogServiceExceptionAsync(documentServiceException);
            }
            catch (PaperlessTaskServiceException paperlessTaskServiceException)
            {
                throw await CreateAndLogServiceExceptionAsync(paperlessTaskServiceException);
            }
            catch (Exception exception)
            {
                var failedDocumentOrchestrationServiceException =
                    new FailedDocumentOrchestrationServiceException(
                        message: "Failed document orchestration service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(
                    failedDocumentOrchestrationServiceException);
            }
        }

        private async ValueTask<DocumentOrchestrationValidationException>
            CreateAndLogValidationExceptionAsync(Xeption exception)
        {
            var documentOrchestrationValidationException =
                new DocumentOrchestrationValidationException(
                    message: "Document orchestration validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentOrchestrationValidationException);

            return documentOrchestrationValidationException;
        }

        private async ValueTask<DocumentOrchestrationDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Xeption exception)
        {
            var documentOrchestrationDependencyValidationException =
                new DocumentOrchestrationDependencyValidationException(
                    message: "Document orchestration dependency validation error occurred, fix the errors and try again.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(
                documentOrchestrationDependencyValidationException);

            return documentOrchestrationDependencyValidationException;
        }

        private async ValueTask<DocumentOrchestrationDependencyException>
            CreateAndLogDependencyExceptionAsync(Xeption exception)
        {
            var documentOrchestrationDependencyException =
                new DocumentOrchestrationDependencyException(
                    message: "Document orchestration dependency error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogCriticalAsync(documentOrchestrationDependencyException);

            return documentOrchestrationDependencyException;
        }

        private async ValueTask<DocumentOrchestrationServiceException>
            CreateAndLogServiceExceptionAsync(Xeption exception)
        {
            var documentOrchestrationServiceException =
                new DocumentOrchestrationServiceException(
                    message: "Document orchestration service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(documentOrchestrationServiceException);

            return documentOrchestrationServiceException;
        }
    }
}
