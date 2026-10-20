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
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();
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
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfConflictErrorOccursAndLogItAsync()
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();
            var httpResponseConflictException = new HttpResponseConflictException();

            var alreadyExistsCustomFieldException =
                new AlreadyExistsCustomFieldException(
                    message: "CustomField already exists.",
                    innerException: httpResponseConflictException);

            var expectedCustomFieldDependencyValidationException =
                new CustomFieldDependencyValidationException(
                    message: "CustomField dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsCustomFieldException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(httpResponseConflictException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfCustomFieldIsNotFoundAndLogItAsync()
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();
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
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
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
        public async Task ShouldThrowDependencyExceptionOnModifyIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();

            var failedCustomFieldDependencyException =
                new FailedCustomFieldDependencyException(
                    message: "Failed customField dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedCustomFieldDependencyException =
                new CustomFieldDependencyException(
                    message: "CustomField dependency error occurred, contact support.",
                    innerException: failedCustomFieldDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
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
        public async Task ShouldThrowDependencyExceptionOnModifyIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();

            var failedCustomFieldDependencyException =
                new FailedCustomFieldDependencyException(
                    message: "Failed customField dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedCustomFieldDependencyException =
                new CustomFieldDependencyException(
                    message: "CustomField dependency error occurred, contact support.",
                    innerException: failedCustomFieldDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnModifyIfTransportErrorOccursAndLogItAsync()
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();
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
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();
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
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            CustomField someCustomField = CreateRandomCustomField();

            var unauthorizedCustomFieldException =
                new UnauthorizedCustomFieldException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedCustomFieldDependencyValidationException =
                new CustomFieldDependencyValidationException(
                    message: "CustomField dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedCustomFieldException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<CustomField>> modifyCustomFieldAsyncTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(someCustomField);

            // then
            await modifyCustomFieldAsyncTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
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
