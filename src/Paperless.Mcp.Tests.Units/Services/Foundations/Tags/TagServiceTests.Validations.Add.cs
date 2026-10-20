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
        public async Task ShouldThrowValidationExceptionOnAddIfTagIsNullAndLogItAsync()
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
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(nullTag!);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnAddIfTagNameIsInvalidAndLogItAsync(
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
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(invalidTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfTagNameExceedsMaximumLengthAndLogItAsync()
        {
            // given
            Tag invalidTag = CreateRandomTag();
            invalidTag.Name = new string('x', count: 129);

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: nameof(Tag.Name),
                value: "Text exceeds the maximum length of 128 characters");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(invalidTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow("a1b2c3")]
        [DataRow("#12345")]
        [DataRow("#gggggg")]
        [DataRow("rot")]
        public async Task ShouldThrowValidationExceptionOnAddIfColorIsInvalidAndLogItAsync(
            string invalidColor)
        {
            // given
            Tag invalidTag = CreateRandomTag();
            invalidTag.Color = invalidColor;

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
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(invalidTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfMatchingAlgorithmIsUndefinedAndLogItAsync()
        {
            // given
            Tag invalidTag = CreateRandomTag();
            invalidTag.MatchingAlgorithm = (TagMatchingAlgorithm)99;

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: nameof(Tag.MatchingAlgorithm),
                value: "Value is not a defined matching algorithm");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(invalidTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
