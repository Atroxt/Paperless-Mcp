using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;
using Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.DocumentTypes
{
    public partial class DocumentTypeProcessingService
    {
        // Die Processing-Schicht reicht die Exceptions der Foundation-Schicht nicht
        // durch, sondern lokalisiert und kategorisiert sie neu.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullDocumentTypeProcessingException nullDocumentTypeProcessingException)
            {
                throw CreateValidationException(nullDocumentTypeProcessingException);
            }
            catch (InvalidDocumentTypeProcessingException invalidDocumentTypeProcessingException)
            {
                throw CreateValidationException(invalidDocumentTypeProcessingException);
            }
            catch (DocumentTypeValidationException documentTypeValidationException)
            {
                throw CreateDependencyValidationException(documentTypeValidationException);
            }
            catch (DocumentTypeDependencyValidationException documentTypeDependencyValidationException)
            {
                throw CreateDependencyValidationException(documentTypeDependencyValidationException);
            }
            catch (DocumentTypeDependencyException documentTypeDependencyException)
            {
                throw CreateDependencyException(documentTypeDependencyException);
            }
            catch (DocumentTypeServiceException documentTypeServiceException)
            {
                throw CreateServiceException(documentTypeServiceException);
            }
            catch (Exception exception)
            {
                var failedDocumentTypeProcessingServiceException =
                    new FailedDocumentTypeProcessingServiceException(
                        message: "Failed documentType processing service error occurred, contact support.",
                        innerException: exception);

                throw CreateServiceException(failedDocumentTypeProcessingServiceException);
            }
        }

        private static DocumentTypeProcessingValidationException CreateValidationException(
            Xeption exception) =>
            new(message: "DocumentType processing validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static DocumentTypeProcessingDependencyValidationException
            CreateDependencyValidationException(Xeption exception) =>
            new(message: "DocumentType processing dependency validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static DocumentTypeProcessingDependencyException CreateDependencyException(
            Xeption exception) =>
            new(message: "DocumentType processing dependency error occurred, contact support.",
                innerException: exception);

        private static DocumentTypeProcessingServiceException CreateServiceException(
            Xeption exception) =>
            new(message: "DocumentType processing service error occurred, contact support.",
                innerException: exception);
    }
}
