using Paperless.Mcp.Models.Foundations.Healths.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.Healths
{
    public partial class HealthService
    {
        private async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
            // 401 und 403 heissen hier: das Token fehlt oder traegt nicht. Das ist eine
            // Frage der Konfiguration, die der Aufrufer beheben kann -- und genau die
            // Auskunft, fuer die es diesen Dienst gibt. Deshalb bewusst nicht kritisch.
            catch (HttpResponseUnauthorizedException httpResponseUnauthorizedException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    httpResponseUnauthorizedException);
            }
            catch (HttpResponseForbiddenException httpResponseForbiddenException)
            {
                throw await CreateAndLogDependencyValidationExceptionAsync(
                    httpResponseForbiddenException);
            }
            catch (HttpResponseUrlNotFoundException httpResponseUrlNotFoundException)
            {
                throw await CreateAndLogCriticalDependencyExceptionAsync(
                    httpResponseUrlNotFoundException);
            }
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
                var failedHealthServiceException =
                    new FailedHealthServiceException(
                        message: "Failed health service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedHealthServiceException);
            }
        }

        private async ValueTask<HealthDependencyValidationException>
            CreateAndLogDependencyValidationExceptionAsync(Exception exception)
        {
            var unauthorizedHealthException =
                new UnauthorizedHealthException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            var healthDependencyValidationException =
                new HealthDependencyValidationException(
                    message: "Health dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedHealthException);

            await this.loggingBroker.LogErrorAsync(healthDependencyValidationException);

            return healthDependencyValidationException;
        }

        private async ValueTask<HealthDependencyException>
            CreateAndLogCriticalDependencyExceptionAsync(Exception exception)
        {
            var failedHealthDependencyException =
                new FailedHealthDependencyException(
                    message: "Failed health dependency error occurred, contact support.",
                    innerException: exception);

            var healthDependencyException =
                new HealthDependencyException(
                    message: "Health dependency error occurred, contact support.",
                    innerException: failedHealthDependencyException);

            await this.loggingBroker.LogCriticalAsync(healthDependencyException);

            return healthDependencyException;
        }

        private async ValueTask<HealthDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            var failedHealthDependencyException =
                new FailedHealthDependencyException(
                    message: "Failed health dependency error occurred, contact support.",
                    innerException: exception);

            var healthDependencyException =
                new HealthDependencyException(
                    message: "Health dependency error occurred, contact support.",
                    innerException: failedHealthDependencyException);

            await this.loggingBroker.LogErrorAsync(healthDependencyException);

            return healthDependencyException;
        }

        private async ValueTask<HealthServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var healthServiceException =
                new HealthServiceException(
                    message: "Health service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(healthServiceException);

            return healthServiceException;
        }
    }
}
