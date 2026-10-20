using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using Paperless.Mcp.Models.Processings.StoragePaths.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.StoragePaths
{
    public partial class StoragePathProcessingService
    {
        // Die Processing-Schicht reicht die Exceptions der Foundation-Schicht nicht
        // durch, sondern lokalisiert und kategorisiert sie neu.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullStoragePathProcessingException nullStoragePathProcessingException)
            {
                throw CreateValidationException(nullStoragePathProcessingException);
            }
            catch (InvalidStoragePathProcessingException invalidStoragePathProcessingException)
            {
                throw CreateValidationException(invalidStoragePathProcessingException);
            }
            catch (StoragePathValidationException storagePathValidationException)
            {
                throw CreateDependencyValidationException(storagePathValidationException);
            }
            catch (StoragePathDependencyValidationException storagePathDependencyValidationException)
            {
                throw CreateDependencyValidationException(storagePathDependencyValidationException);
            }
            catch (StoragePathDependencyException storagePathDependencyException)
            {
                throw CreateDependencyException(storagePathDependencyException);
            }
            catch (StoragePathServiceException storagePathServiceException)
            {
                throw CreateServiceException(storagePathServiceException);
            }
            catch (Exception exception)
            {
                var failedStoragePathProcessingServiceException =
                    new FailedStoragePathProcessingServiceException(
                        message: "Failed storagePath processing service error occurred, contact support.",
                        innerException: exception);

                throw CreateServiceException(failedStoragePathProcessingServiceException);
            }
        }

        private static StoragePathProcessingValidationException CreateValidationException(
            Xeption exception) =>
            new(message: "StoragePath processing validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static StoragePathProcessingDependencyValidationException
            CreateDependencyValidationException(Xeption exception) =>
            new(message: "StoragePath processing dependency validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static StoragePathProcessingDependencyException CreateDependencyException(
            Xeption exception) =>
            new(message: "StoragePath processing dependency error occurred, contact support.",
                innerException: exception);

        private static StoragePathProcessingServiceException CreateServiceException(
            Xeption exception) =>
            new(message: "StoragePath processing service error occurred, contact support.",
                innerException: exception);
    }
}
