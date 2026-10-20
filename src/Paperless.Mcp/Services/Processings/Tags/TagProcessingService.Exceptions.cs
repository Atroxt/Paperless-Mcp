using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using Paperless.Mcp.Models.Processings.Tags.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.Tags
{
    public partial class TagProcessingService
    {
        // Die Processing-Schicht reicht die Exceptions der Foundation-Schicht nicht
        // durch, sondern lokalisiert und kategorisiert sie neu.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullTagProcessingException nullTagProcessingException)
            {
                throw CreateValidationException(nullTagProcessingException);
            }
            catch (InvalidTagProcessingException invalidTagProcessingException)
            {
                throw CreateValidationException(invalidTagProcessingException);
            }
            catch (TagValidationException tagValidationException)
            {
                throw CreateDependencyValidationException(tagValidationException);
            }
            catch (TagDependencyValidationException tagDependencyValidationException)
            {
                throw CreateDependencyValidationException(tagDependencyValidationException);
            }
            catch (TagDependencyException tagDependencyException)
            {
                throw CreateDependencyException(tagDependencyException);
            }
            catch (TagServiceException tagServiceException)
            {
                throw CreateServiceException(tagServiceException);
            }
            catch (Exception exception)
            {
                var failedTagProcessingServiceException =
                    new FailedTagProcessingServiceException(
                        message: "Failed tag processing service error occurred, contact support.",
                        innerException: exception);

                throw CreateServiceException(failedTagProcessingServiceException);
            }
        }

        private static TagProcessingValidationException CreateValidationException(
            Xeption exception) =>
            new(message: "Tag processing validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static TagProcessingDependencyValidationException
            CreateDependencyValidationException(Xeption exception) =>
            new(message: "Tag processing dependency validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static TagProcessingDependencyException CreateDependencyException(
            Xeption exception) =>
            new(message: "Tag processing dependency error occurred, contact support.",
                innerException: exception);

        private static TagProcessingServiceException CreateServiceException(
            Xeption exception) =>
            new(message: "Tag processing service error occurred, contact support.",
                innerException: exception);
    }
}
