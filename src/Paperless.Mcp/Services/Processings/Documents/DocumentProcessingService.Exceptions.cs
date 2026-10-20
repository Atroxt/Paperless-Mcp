using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.Documents
{
    public partial class DocumentProcessingService
    {
        // Die Processing-Schicht reicht die Exceptions der Foundation-Schicht nicht
        // durch, sondern lokalisiert und kategorisiert sie neu.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullDocumentProcessingException nullDocumentProcessingException)
            {
                throw CreateValidationException(nullDocumentProcessingException);
            }
            catch (InvalidDocumentProcessingException invalidDocumentProcessingException)
            {
                throw CreateValidationException(invalidDocumentProcessingException);
            }
            catch (DocumentValidationException documentValidationException)
            {
                throw CreateDependencyValidationException(documentValidationException);
            }
            catch (DocumentDependencyValidationException documentDependencyValidationException)
            {
                throw CreateDependencyValidationException(documentDependencyValidationException);
            }
            catch (DocumentDependencyException documentDependencyException)
            {
                throw CreateDependencyException(documentDependencyException);
            }
            catch (DocumentServiceException documentServiceException)
            {
                throw CreateServiceException(documentServiceException);
            }
            catch (Exception exception)
            {
                var failedDocumentProcessingServiceException =
                    new FailedDocumentProcessingServiceException(
                        message: "Failed document processing service error occurred, contact support.",
                        innerException: exception);

                throw CreateServiceException(failedDocumentProcessingServiceException);
            }
        }

        private static DocumentProcessingValidationException CreateValidationException(
            Xeption exception) =>
            new(message: "Document processing validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static DocumentProcessingDependencyValidationException
            CreateDependencyValidationException(Xeption exception) =>
            new(message: "Document processing dependency validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static DocumentProcessingDependencyException CreateDependencyException(
            Xeption exception) =>
            new(message: "Document processing dependency error occurred, contact support.",
                innerException: exception);

        private static DocumentProcessingServiceException CreateServiceException(
            Xeption exception) =>
            new(message: "Document processing service error occurred, contact support.",
                innerException: exception);
    }
}
