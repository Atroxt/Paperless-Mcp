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
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRemoveByIdIfIdIsInvalidAndLogItAsync(
            int invalidCorrespondentId)
        {
            // given
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
            Func<Task> removeCorrespondentByIdTask = async () =>
                await this.correspondentService.RemoveCorrespondentByIdAsync(invalidCorrespondentId);

            // then
            await removeCorrespondentByIdTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
