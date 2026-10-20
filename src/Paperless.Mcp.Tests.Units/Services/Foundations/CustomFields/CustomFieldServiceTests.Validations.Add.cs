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
        public async Task ShouldThrowValidationExceptionOnAddIfCustomFieldIsNullAndLogItAsync()
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
            Func<Task<CustomField>> addCustomFieldTask = async () =>
                await this.customFieldService.AddCustomFieldAsync(nullCustomField!);

            // then
            await addCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnAddIfCustomFieldNameIsInvalidAndLogItAsync(
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
            Func<Task<CustomField>> addCustomFieldTask = async () =>
                await this.customFieldService.AddCustomFieldAsync(invalidCustomField);

            // then
            await addCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfSelectOptionsAreMissingAndLogItAsync()
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
            Func<Task<CustomField>> addCustomFieldTask = async () =>
                await this.customFieldService.AddCustomFieldAsync(invalidCustomField);

            // then
            await addCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfCustomFieldNameExceedsMaximumLengthAndLogItAsync()
        {
            // given
            CustomField invalidCustomField = CreateRandomCustomField();
            invalidCustomField.Name = new string('x', count: 129);

            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            invalidCustomFieldException.UpsertDataList(
                key: nameof(CustomField.Name),
                value: "Text exceeds the maximum length of 128 characters");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            // when
            Func<Task<CustomField>> addCustomFieldTask = async () =>
                await this.customFieldService.AddCustomFieldAsync(invalidCustomField);

            // then
            await addCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfDataTypeIsUndefinedAndLogItAsync()
        {
            // given
            CustomField invalidCustomField = CreateRandomCustomField();
            invalidCustomField.DataType = (CustomFieldDataType)99;

            var invalidCustomFieldException =
                new InvalidCustomFieldException(
                    message: "CustomField is invalid, fix the errors and try again.");

            invalidCustomFieldException.UpsertDataList(
                key: nameof(CustomField.DataType),
                value: "Value is not a defined data type");

            var expectedCustomFieldValidationException =
                new CustomFieldValidationException(
                    message: "CustomField validation error occurred, fix the errors and try again.",
                    innerException: invalidCustomFieldException);

            // when
            Func<Task<CustomField>> addCustomFieldTask = async () =>
                await this.customFieldService.AddCustomFieldAsync(invalidCustomField);

            // then
            await addCustomFieldTask.Should()
                .ThrowAsync<CustomFieldValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCustomFieldValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCustomFieldValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCustomFieldAsync(It.IsAny<CustomField>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
