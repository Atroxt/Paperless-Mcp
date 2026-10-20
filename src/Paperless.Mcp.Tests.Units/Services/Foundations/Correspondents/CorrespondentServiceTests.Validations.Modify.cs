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
        public async Task ShouldThrowValidationExceptionOnModifyIfCorrespondentIsNullAndLogItAsync()
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
            Func<Task<Correspondent>> modifyCorrespondentTask = async () =>
                await this.correspondentService.ModifyCorrespondentAsync(nullCorrespondent!);

            // then
            await modifyCorrespondentTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCorrespondentAsync(It.IsAny<Correspondent>()),
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
            Correspondent invalidCorrespondent = CreateRandomCorrespondent();
            invalidCorrespondent.Id = invalidId;

            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.");

            invalidCorrespondentException.UpsertDataList(
                key: nameof(Correspondent.Id),
                value: "Value must be greater than zero");

            var expectedCorrespondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: invalidCorrespondentException);

            // when
            Func<Task<Correspondent>> modifyCorrespondentTask = async () =>
                await this.correspondentService.ModifyCorrespondentAsync(invalidCorrespondent);

            // then
            await modifyCorrespondentTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnModifyIfCorrespondentNameIsInvalidAndLogItAsync(
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
            Func<Task<Correspondent>> modifyCorrespondentTask = async () =>
                await this.correspondentService.ModifyCorrespondentAsync(invalidCorrespondent);

            // then
            await modifyCorrespondentTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCorrespondentAsync(It.IsAny<Correspondent>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
