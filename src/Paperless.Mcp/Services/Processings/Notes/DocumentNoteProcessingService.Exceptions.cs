using Paperless.Mcp.Models.Foundations.Notes.Exceptions;
using Paperless.Mcp.Models.Processings.Notes.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.Notes
{
    public partial class DocumentNoteProcessingService
    {
        // Die Processing-Schicht reicht die Exceptions der Foundation-Schicht nicht
        // durch, sondern lokalisiert und kategorisiert sie neu.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullDocumentNoteProcessingException nullDocumentNoteProcessingException)
            {
                throw CreateValidationException(nullDocumentNoteProcessingException);
            }
            catch (InvalidDocumentNoteProcessingException invalidDocumentNoteProcessingException)
            {
                throw CreateValidationException(invalidDocumentNoteProcessingException);
            }
            catch (DocumentNoteValidationException documentNoteValidationException)
            {
                throw CreateDependencyValidationException(documentNoteValidationException);
            }
            catch (DocumentNoteDependencyValidationException dependencyValidationException)
            {
                throw CreateDependencyValidationException(dependencyValidationException);
            }
            catch (DocumentNoteDependencyException documentNoteDependencyException)
            {
                throw CreateDependencyException(documentNoteDependencyException);
            }
            catch (DocumentNoteServiceException documentNoteServiceException)
            {
                throw CreateServiceException(documentNoteServiceException);
            }
            catch (Exception exception)
            {
                var failedDocumentNoteProcessingServiceException =
                    new FailedDocumentNoteProcessingServiceException(
                        message: "Failed document note processing service error occurred, "
                            + "contact support.",
                        innerException: exception);

                throw CreateServiceException(failedDocumentNoteProcessingServiceException);
            }
        }

        private static DocumentNoteProcessingValidationException CreateValidationException(
            Xeption exception) =>
            new(message: "Document note processing validation error occurred, "
                    + "fix the errors and try again.",
                innerException: exception);

        private static DocumentNoteProcessingDependencyValidationException
            CreateDependencyValidationException(Xeption exception) =>
            new(message: "Document note processing dependency validation error occurred, "
                    + "fix the errors and try again.",
                innerException: exception);

        private static DocumentNoteProcessingDependencyException CreateDependencyException(
            Xeption exception) =>
            new(message: "Document note processing dependency error occurred, contact support.",
                innerException: exception);

        private static DocumentNoteProcessingServiceException CreateServiceException(
            Xeption exception) =>
            new(message: "Document note processing service error occurred, contact support.",
                innerException: exception);
    }
}
