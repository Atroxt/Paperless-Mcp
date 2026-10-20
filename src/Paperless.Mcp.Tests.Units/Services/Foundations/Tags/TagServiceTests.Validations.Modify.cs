using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    public partial class TagServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyIfTagIsNullAndLogItAsync()
        {
            // given
            Tag? nullTag = null;

            var nullTagException =
                new NullTagException(message: "Tag is null.");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: nullTagException);

            // when
            Func<Task<Tag>> modifyTagTask = async () =>
                await this.tagService.ModifyTagAsync(nullTag!);

            // then
            await modifyTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutTagAsync(It.IsAny<Tag>()),
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
            Tag invalidTag = CreateRandomTag();
            invalidTag.Id = invalidId;

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: nameof(Tag.Id),
                value: "Value must be greater than zero");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            // when
            Func<Task<Tag>> modifyTagTask = async () =>
                await this.tagService.ModifyTagAsync(invalidTag);

            // then
            await modifyTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnModifyIfTagNameIsInvalidAndLogItAsync(
            string? invalidName)
        {
            // given
            Tag invalidTag = CreateRandomTag();
            invalidTag.Name = invalidName!;

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: nameof(Tag.Name),
                value: "Text is required");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            // when
            Func<Task<Tag>> modifyTagTask = async () =>
                await this.tagService.ModifyTagAsync(invalidTag);

            // then
            await modifyTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyIfColorIsInvalidAndLogItAsync()
        {
            // given
            Tag invalidTag = CreateRandomTag();
            invalidTag.Color = "nicht-hex";

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: nameof(Tag.Color),
                value: "Color must be a hex value such as #a1b2c3");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            // when
            Func<Task<Tag>> modifyTagTask = async () =>
                await this.tagService.ModifyTagAsync(invalidTag);

            // then
            await modifyTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
