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
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageIsInvalidAndLogItAsync(
            int invalidPage)
        {
            // given
            int validPageSize = GetRandomNumber();

            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.");

            invalidCorrespondentException.UpsertDataList(
                key: "Page",
                value: "Value must be greater than zero");

            var expectedCorrespondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: invalidCorrespondentException);

            // when
            Func<Task<CorrespondentPage>> retrieveCorrespondentPageTask = async () =>
                await this.correspondentService.RetrieveCorrespondentPageAsync(invalidPage, validPageSize);

            // then
            await retrieveCorrespondentPageTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCorrespondentPageAsync(It.IsAny<int>(), It.IsAny<int>()),
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

            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.");

            invalidCorrespondentException.UpsertDataList(
                key: "PageSize",
                value: "Value must be greater than zero");

            var expectedCorrespondentValidationException =
                new CorrespondentValidationException(
                    message: "Correspondent validation error occurred, fix the errors and try again.",
                    innerException: invalidCorrespondentException);

            // when
            Func<Task<CorrespondentPage>> retrieveCorrespondentPageTask = async () =>
                await this.correspondentService.RetrieveCorrespondentPageAsync(validPage, invalidPageSize);

            // then
            await retrieveCorrespondentPageTask.Should()
                .ThrowAsync<CorrespondentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedCorrespondentValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedCorrespondentValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCorrespondentPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
