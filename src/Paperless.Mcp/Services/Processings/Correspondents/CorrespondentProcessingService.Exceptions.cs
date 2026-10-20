using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;
using Paperless.Mcp.Models.Processings.Correspondents.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.Correspondents
{
    public partial class CorrespondentProcessingService
    {
        // Die Processing-Schicht reicht die Exceptions der Foundation-Schicht nicht
        // durch, sondern lokalisiert und kategorisiert sie neu.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullCorrespondentProcessingException nullCorrespondentProcessingException)
            {
                throw CreateValidationException(nullCorrespondentProcessingException);
            }
            catch (InvalidCorrespondentProcessingException invalidCorrespondentProcessingException)
            {
                throw CreateValidationException(invalidCorrespondentProcessingException);
            }
            catch (CorrespondentValidationException correspondentValidationException)
            {
                throw CreateDependencyValidationException(correspondentValidationException);
            }
            catch (CorrespondentDependencyValidationException correspondentDependencyValidationException)
            {
                throw CreateDependencyValidationException(correspondentDependencyValidationException);
            }
            catch (CorrespondentDependencyException correspondentDependencyException)
            {
                throw CreateDependencyException(correspondentDependencyException);
            }
            catch (CorrespondentServiceException correspondentServiceException)
            {
                throw CreateServiceException(correspondentServiceException);
            }
            catch (Exception exception)
            {
                var failedCorrespondentProcessingServiceException =
                    new FailedCorrespondentProcessingServiceException(
                        message: "Failed correspondent processing service error occurred, contact support.",
                        innerException: exception);

                throw CreateServiceException(failedCorrespondentProcessingServiceException);
            }
        }

        private static CorrespondentProcessingValidationException CreateValidationException(
            Xeption exception) =>
            new(message: "Correspondent processing validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static CorrespondentProcessingDependencyValidationException
            CreateDependencyValidationException(Xeption exception) =>
            new(message: "Correspondent processing dependency validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static CorrespondentProcessingDependencyException CreateDependencyException(
            Xeption exception) =>
            new(message: "Correspondent processing dependency error occurred, contact support.",
                innerException: exception);

        private static CorrespondentProcessingServiceException CreateServiceException(
            Xeption exception) =>
            new(message: "Correspondent processing service error occurred, contact support.",
                innerException: exception);
    }
}
