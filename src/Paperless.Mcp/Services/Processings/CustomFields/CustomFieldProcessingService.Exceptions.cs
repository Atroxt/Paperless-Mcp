using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;
using Paperless.Mcp.Models.Processings.CustomFields.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Processings.CustomFields
{
    public partial class CustomFieldProcessingService
    {
        // Die Processing-Schicht reicht die Exceptions der Foundation-Schicht nicht
        // durch, sondern lokalisiert und kategorisiert sie neu.
        private static async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            catch (NullCustomFieldProcessingException nullCustomFieldProcessingException)
            {
                throw CreateValidationException(nullCustomFieldProcessingException);
            }
            catch (InvalidCustomFieldProcessingException invalidCustomFieldProcessingException)
            {
                throw CreateValidationException(invalidCustomFieldProcessingException);
            }
            catch (CustomFieldValidationException customFieldValidationException)
            {
                throw CreateDependencyValidationException(customFieldValidationException);
            }
            catch (CustomFieldDependencyValidationException customFieldDependencyValidationException)
            {
                throw CreateDependencyValidationException(customFieldDependencyValidationException);
            }
            catch (CustomFieldDependencyException customFieldDependencyException)
            {
                throw CreateDependencyException(customFieldDependencyException);
            }
            catch (CustomFieldServiceException customFieldServiceException)
            {
                throw CreateServiceException(customFieldServiceException);
            }
            catch (Exception exception)
            {
                var failedCustomFieldProcessingServiceException =
                    new FailedCustomFieldProcessingServiceException(
                        message: "Failed customField processing service error occurred, contact support.",
                        innerException: exception);

                throw CreateServiceException(failedCustomFieldProcessingServiceException);
            }
        }

        private static CustomFieldProcessingValidationException CreateValidationException(
            Xeption exception) =>
            new(message: "CustomField processing validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static CustomFieldProcessingDependencyValidationException
            CreateDependencyValidationException(Xeption exception) =>
            new(message: "CustomField processing dependency validation error occurred, fix the errors and try again.",
                innerException: exception);

        private static CustomFieldProcessingDependencyException CreateDependencyException(
            Xeption exception) =>
            new(message: "CustomField processing dependency error occurred, contact support.",
                innerException: exception);

        private static CustomFieldProcessingServiceException CreateServiceException(
            Xeption exception) =>
            new(message: "CustomField processing service error occurred, contact support.",
                innerException: exception);
    }
}
