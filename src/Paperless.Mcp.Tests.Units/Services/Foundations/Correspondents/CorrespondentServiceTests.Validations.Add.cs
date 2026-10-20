using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;

using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfCorrespondentIsNullAndLogItAsync()
        {
            // given
            Correspondent? nullCorrespondent = null;

            var nullCorrespondentException =
                new NullCorrespondentException(message: "Correspondent is null.");

            var expectedCorrespondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: nullCorrespondentException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(nullCorrespondent!);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnAddIfCorrespondentNameIsInvalidAndLogItAsync(
            string? invalidName)
        {
            // given
            Correspondent invalidCorrespondent = CreateRandomCorrespondent();
            invalidCorrespondent.Name = invalidName!;

            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.");

            invalidCorrespondentException.UpsertDataList(
                key: nameof(Correspondent.Name),
                value: "Text is required");

            var expectedCorrespondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: invalidCorrespondentException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(invalidCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfCorrespondentNameExceedsMaximumLengthAndLogItAsync()
        {
            // given
            Correspondent invalidCorrespondent = CreateRandomCorrespondent();
            invalidCorrespondent.Name = new string('x', count: 129);

            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.");

            invalidCorrespondentException.UpsertDataList(
                key: nameof(Correspondent.Name),
                value: "Text exceeds the maximum length of 128 characters");

            var expectedCorrespondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: invalidCorrespondentException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(invalidCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfMatchingAlgorithmIsUndefinedAndLogItAsync()
        {
            // given
            Correspondent invalidCorrespondent = CreateRandomCorrespondent();
            invalidCorrespondent.MatchingAlgorithm = (CorrespondentMatchingAlgorithm)99;

            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.");

            invalidCorrespondentException.UpsertDataList(
                key: nameof(Correspondent.MatchingAlgorithm),
                value: "Value is not a defined matching algorithm");

            var expectedCorrespondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: invalidCorrespondentException);

            // when
            Func<Task<Correspondent>> addCorrespondentTask = async () =>
                await this.correspondentService.AddCorrespondentAsync(invalidCorrespondent);

            // then
            await addCorrespondentTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
