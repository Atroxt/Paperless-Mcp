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
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageIsInvalidAndLogItAsync(
            int invalidPage)
        {
            // given
            int validPageSize = GetRandomNumber();

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: "Page",
                value: "Value must be greater than zero");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(invalidPage, validPageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
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

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: "PageSize",
                value: "Value must be greater than zero");

            var expectedTagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(validPage, invalidPageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
