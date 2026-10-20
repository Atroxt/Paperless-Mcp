using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    public partial class CustomFieldServiceTests
    {
        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageIsInvalidAndLogItAsync(
            int invalidPage)
        {
            // given
            int validPageSize = GetRandomNumber();

            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            invalidCustomFieldException.UpsertDataList(
                key: "Page",
                value: "Value must be greater than zero");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(invalidPage, validPageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageSizeIsInvalidAndLogItAsync(
            int invalidPageSize)
        {
            // given
            int validPage = GetRandomNumber();

            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            invalidCustomFieldException.UpsertDataList(
                key: "PageSize",
                value: "Value must be greater than zero");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            // when
            Func<Task<CustomFieldPage>> retrieveCustomFieldPageTask = async () =>
                await this.customFieldService.RetrieveCustomFieldPageAsync(validPage, invalidPageSize);

            // then
            await retrieveCustomFieldPageTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
