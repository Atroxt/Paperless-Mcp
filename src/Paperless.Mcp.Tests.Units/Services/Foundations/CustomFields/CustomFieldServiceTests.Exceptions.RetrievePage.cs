using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    public partial class CustomFieldServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedCustomFieldDependencyValidationException =
                new CustomFieldDependencyValidationException(
                    message: "CustomField dependency validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(somePage, somePageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Paperless antwortet mit 404, wenn die angefragte Seite ausserhalb des Bereichs liegt.
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfPageIsOutOfRangeAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundCustomFieldException =
                new NotFoundCustomFieldException(
                    message: "CustomField not found.",
                    innerException: httpResponseNotFoundException);

            var expectedCustomFieldDependencyValidationException =
                new CustomFieldDependencyValidationException(
                    message: "CustomField dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundCustomFieldException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(somePage, somePageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(CriticalDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var failedCustomFieldDependencyException =
                new FailedCustomFieldDependencyException(
                    message: "Failed customField dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedCustomFieldDependencyException =
                new CustomFieldDependencyException(
                    message: "CustomField dependency error occurred, contact support.",
                    innerException: failedCustomFieldDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(somePage, somePageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var failedCustomFieldDependencyException =
                new FailedCustomFieldDependencyException(
                    message: "Failed customField dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedCustomFieldDependencyException =
                new CustomFieldDependencyException(
                    message: "CustomField dependency error occurred, contact support.",
                    innerException: failedCustomFieldDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(somePage, somePageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfTransportErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpRequestException = new HttpRequestException();

            var failedCustomFieldDependencyException =
                new FailedCustomFieldDependencyException(
                    message: "Failed customField dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedCustomFieldDependencyException =
                new CustomFieldDependencyException(
                    message: "CustomField dependency error occurred, contact support.",
                    innerException: failedCustomFieldDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(somePage, somePageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrievePageIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var serviceException = new Exception();

            var failedCustomFieldServiceException =
                new FailedCustomFieldServiceException(
                    message: "Failed customField service error occurred, contact support.",
                    innerException: serviceException);

            var expectedCustomFieldServiceException =
                new CustomFieldServiceException(
                    message: "CustomField service error occurred, contact support.",
                    innerException: failedCustomFieldServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(somePage, somePageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var unauthorizedCustomFieldException =
                new UnauthorizedCustomFieldException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedCustomFieldDependencyValidationException =
                new CustomFieldDependencyValidationException(
                    message: "CustomField dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedCustomFieldException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageAsyncTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(somePage, somePageSize);

            // then
            await retrieveCustomFieldPageAsyncTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

    }
}
