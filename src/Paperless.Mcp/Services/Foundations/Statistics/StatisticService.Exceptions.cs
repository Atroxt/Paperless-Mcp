using Paperless.Mcp.Models.Foundations.Statistics.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Services.Foundations.Statistics
{
    public partial class StatisticService
    {
        // Ohne Eingabe gibt es keine Validierung und keinen 400/404/409 auf eine
        // Ressource -- uebrig bleiben Token- und Infrastrukturfehler.
        private async ValueTask<T> TryCatch<T>(Func<ValueTask<T>> returningFunction)
        {
            try
            {
                return await returningFunction();
            }
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
                var failedStatisticServiceException =
                    new FailedStatisticServiceException(
                        message: "Failed statistic service error occurred, contact support.",
                        innerException: exception);

                throw await CreateAndLogServiceExceptionAsync(failedStatisticServiceException);
            }
        }

        private async ValueTask<StatisticDependencyValidationException>
            CreateAndLogUnauthorizedExceptionAsync(Exception exception)
        {
            var unauthorizedStatisticException =
                new UnauthorizedStatisticException(
                    message: "Paperless rejected the API token.",
                    innerException: exception);

            var statisticDependencyValidationException =
                new StatisticDependencyValidationException(
                    message: "Statistic dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedStatisticException);

            await this.loggingBroker.LogErrorAsync(statisticDependencyValidationException);

            return statisticDependencyValidationException;
        }

        private async ValueTask<StatisticDependencyException> CreateAndLogCriticalDependencyExceptionAsync(
            Exception exception)
        {
            StatisticDependencyException statisticDependencyException =
                CreateDependencyException(exception);

            await this.loggingBroker.LogCriticalAsync(statisticDependencyException);

            return statisticDependencyException;
        }

        private async ValueTask<StatisticDependencyException> CreateAndLogDependencyExceptionAsync(
            Exception exception)
        {
            StatisticDependencyException statisticDependencyException =
                CreateDependencyException(exception);

            await this.loggingBroker.LogErrorAsync(statisticDependencyException);

            return statisticDependencyException;
        }

        private static StatisticDependencyException CreateDependencyException(Exception exception)
        {
            var failedStatisticDependencyException =
                new FailedStatisticDependencyException(
                    message: "Failed statistic dependency error occurred, contact support.",
                    innerException: exception);

            return new StatisticDependencyException(
                message: "Statistic dependency error occurred, contact support.",
                innerException: failedStatisticDependencyException);
        }

        private async ValueTask<StatisticServiceException> CreateAndLogServiceExceptionAsync(
            Xeption exception)
        {
            var statisticServiceException =
                new StatisticServiceException(
                    message: "Statistic service error occurred, contact support.",
                    innerException: exception);

            await this.loggingBroker.LogErrorAsync(statisticServiceException);

            return statisticServiceException;
        }
    }
}
