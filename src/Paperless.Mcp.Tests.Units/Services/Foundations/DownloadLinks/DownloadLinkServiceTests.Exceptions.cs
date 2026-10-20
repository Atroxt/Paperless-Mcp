using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DownloadLinks
{
    public partial class DownloadLinkServiceTests
    {
        // Diese Service spricht keine fremde API an -- ein Fehler des Vorrats ist deshalb
        // immer unerwartet und landet im Catch-All.
        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnIssueIfServiceErrorOccursAndLogItAsync()
        {
            // given
            var serviceException = new Exception();

            var failedDownloadLinkServiceException =
                new FailedDownloadLinkServiceException(
                    message: "Failed download link service error occurred, contact support.",
                    innerException: serviceException);

            var expectedDownloadLinkServiceException =
                new DownloadLinkServiceException(
                    message: "DownloadLink service error occurred, contact support.",
                    innerException: failedDownloadLinkServiceException);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<DownloadLink>> issueTask = async () =>
                await this.downloadLinkService.IssueDownloadLinkAsync(
                    GetRandomNumber(), original: false, token: GetRandomToken());

            // then
            await issueTask.Should()
                .ThrowAsync<DownloadLinkServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDownloadLinkServiceException));

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDownloadLinkServiceException))),
                        Times.Once);

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.StoreAsync(It.IsAny<DownloadLink>()),
                    Times.Never);

            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRedeemIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string randomId = GetRandomId();
            var serviceException = new Exception();

            var failedDownloadLinkServiceException =
                new FailedDownloadLinkServiceException(
                    message: "Failed download link service error occurred, contact support.",
                    innerException: serviceException);

            var expectedDownloadLinkServiceException =
                new DownloadLinkServiceException(
                    message: "DownloadLink service error occurred, contact support.",
                    innerException: failedDownloadLinkServiceException);

            this.downloadLinkBrokerMock.Setup(broker =>
                broker.TakeAsync(randomId))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<DownloadLink>> redeemTask = async () =>
                await this.downloadLinkService.RedeemDownloadLinkAsync(randomId);

            // then
            await redeemTask.Should()
                .ThrowAsync<DownloadLinkServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDownloadLinkServiceException));

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(randomId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDownloadLinkServiceException))),
                        Times.Once);

            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}
