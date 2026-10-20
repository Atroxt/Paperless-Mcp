using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DownloadLinks
{
    public partial class DownloadLinkServiceTests
    {
        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnIssueIfDocumentIdIsInvalidAndLogItAsync(
            int invalidDocumentId)
        {
            // given
            var invalidDownloadLinkException =
                new InvalidDownloadLinkException(
                    message: "DownloadLink is invalid, fix the errors and try again.");

            invalidDownloadLinkException.UpsertDataList(
                key: nameof(DownloadLink.DocumentId),
                value: "Value must be greater than zero");

            var expectedDownloadLinkValidationException =
                new DownloadLinkValidationException(
                    message: "DownloadLink validation error occurred, fix the errors and try again.",
                    innerException: invalidDownloadLinkException);

            // when
            Func<Task<DownloadLink>> issueTask = async () =>
                await this.downloadLinkService.IssueDownloadLinkAsync(
                    invalidDocumentId, original: false, token: GetRandomToken());

            // then
            await issueTask.Should()
                .ThrowAsync<DownloadLinkValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDownloadLinkValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDownloadLinkValidationException))),
                        Times.Once);

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.StoreAsync(It.IsAny<DownloadLink>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        // Der wichtigste Validierungstest dieser Service: ohne Token des Aufrufers wird
        // kein Link ausgestellt. Gaebe es hier einen Rueckfall auf das Token des
        // Betreibers, koennte sich jeder einen Link auf fremde Dokumente holen.
        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnIssueIfTokenIsInvalidAndLogItAsync(
            string invalidToken)
        {
            // given
            var invalidDownloadLinkException =
                new InvalidDownloadLinkException(
                    message: "DownloadLink is invalid, fix the errors and try again.");

            invalidDownloadLinkException.UpsertDataList(
                key: nameof(DownloadLink.Token),
                value: "Text is required");

            var expectedDownloadLinkValidationException =
                new DownloadLinkValidationException(
                    message: "DownloadLink validation error occurred, fix the errors and try again.",
                    innerException: invalidDownloadLinkException);

            // when
            Func<Task<DownloadLink>> issueTask = async () =>
                await this.downloadLinkService.IssueDownloadLinkAsync(
                    GetRandomNumber(), original: false, token: invalidToken);

            // then
            await issueTask.Should()
                .ThrowAsync<DownloadLinkValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDownloadLinkValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDownloadLinkValidationException))),
                        Times.Once);

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.StoreAsync(It.IsAny<DownloadLink>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }
    }
}
