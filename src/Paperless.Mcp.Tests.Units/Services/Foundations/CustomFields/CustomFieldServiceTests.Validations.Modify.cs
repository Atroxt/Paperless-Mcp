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
        public async Task ShouldThrowValidationExceptionOnModifyIfCustomFieldIsNullAndLogItAsync()
        {
            // given
            CustomField? nullCustomField = null;

            var nullCustomFieldException =
                new NullCustomFieldException(message: "CustomField is null.");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: nullCustomFieldException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(nullCustomField!);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnModifyIfIdIsInvalidAndLogItAsync(
            int invalidId)
        {
            // given
            CustomField invalidCustomField = CreateRandomCustomField();
            invalidCustomField.Id = invalidId;

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
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(invalidCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnModifyIfCustomFieldNameIsInvalidAndLogItAsync(
            string? invalidName)
        {
            // given
            CustomField invalidCustomField = CreateRandomCustomField();
            invalidCustomField.Name = invalidName!;

            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            invalidCustomFieldException.UpsertDataList(
                key: nameof(CustomField.Name),
                value: "Text is required");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(invalidCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyIfSelectOptionsAreMissingAndLogItAsync()
        {
            // given
            CustomField invalidCustomField = CreateRandomCustomField();
            invalidCustomField.DataType = CustomFieldDataType.Select;
            invalidCustomField.ExtraData = null;

            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            invalidCustomFieldException.UpsertDataList(
                key: nameof(CustomField.ExtraData),
                value: "Select options are required for a select field");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            // when
            Func<Task<CustomField>> modifyCustomFieldTask = async () =>
                await this.customFieldService.ModifyCustomFieldAsync(invalidCustomField);

            // then
            await modifyCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
