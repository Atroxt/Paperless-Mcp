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
        public async Task ShouldThrowValidationExceptionOnRemoveByIdIfIdIsInvalidAndLogItAsync(
            int invalidCustomFieldId)
        {
            // given
            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            invalidCustomFieldException.UpsertDataList(
                key: nameof(CustomField.Id),
                value: "Value must be greater than zero");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            // when
            Func<Task> removeCustomFieldByIdTask = async () =>
                await this.customFieldService.RemoveCustomFieldByIdAsync(invalidCustomFieldId);

            // then
            await removeCustomFieldByIdTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCustomFieldByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
