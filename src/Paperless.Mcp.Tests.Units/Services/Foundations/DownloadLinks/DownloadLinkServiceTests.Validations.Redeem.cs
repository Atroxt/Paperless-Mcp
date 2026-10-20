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
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnRedeemIfIdIsInvalidAndLogItAsync(
            string invalidId)
        {
            // given
            var invalidDownloadLinkException =
                new InvalidDownloadLinkException(
                    message: "DownloadLink is invalid, fix the errors and try again.");

            invalidDownloadLinkException.UpsertDataList(
                key: nameof(DownloadLink.Id),
                value: "Text is required");

            var expectedDownloadLinkValidationException =
                new DownloadLinkValidationException(
                    message: "DownloadLink validation error occurred, fix the errors and try again.",
                    innerException: invalidDownloadLinkException);

            // when
            Func<Task<DownloadLink>> redeemTask = async () =>
                await this.downloadLinkService.RedeemDownloadLinkAsync(invalidId);

            // then
            await redeemTask.Should()
                .ThrowAsync<DownloadLinkValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDownloadLinkValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDownloadLinkValidationException))),
                        Times.Once);

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(It.IsAny<string>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        // Einmalig heisst: beim zweiten Mal ist der Link weg. Der Broker gibt dann null
        // zurueck, und das muss derselbe Fehler sein wie bei einer erfundenen Kennung --
        // von aussen darf nicht unterscheidbar sein, ob es den Link je gab.
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRedeemIfLinkIsUnknownAndLogItAsync()
        {
            // given
            string randomId = GetRandomId();

            var notFoundDownloadLinkException =
                new NotFoundDownloadLinkException(
                    message: "Download link is unknown or was already used.");

            var expectedDownloadLinkDependencyValidationException =
                new DownloadLinkDependencyValidationException(
                    message: "DownloadLink dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundDownloadLinkException);

            this.downloadLinkBrokerMock.Setup(broker =>
                broker.TakeAsync(randomId))
                    .ReturnsAsync((DownloadLink?)null);

            // when
            Func<Task<DownloadLink>> redeemTask = async () =>
                await this.downloadLinkService.RedeemDownloadLinkAsync(randomId);

            // then
            await redeemTask.Should()
                .ThrowAsync<DownloadLinkDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(
                        expectedDownloadLinkDependencyValidationException));

            this.downloadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(randomId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDownloadLinkDependencyValidationException))),
                        Times.Once);

            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRedeemIfLinkExpiredAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            // Abgelaufen heisst hier: genau jetzt. Die Grenze gehoert dem Ablauf, nicht
            // dem Aufrufer.
            DownloadLink expiredDownloadLink =
                CreateRandomDownloadLink(randomDateTimeOffset);

            string inputId = expiredDownloadLink.Id;

            var notFoundDownloadLinkException =
                new NotFoundDownloadLinkException(
                    message: "Download link has expired.");

            var expectedDownloadLinkDependencyValidationException =
                new DownloadLinkDependencyValidationException(
                    message: "DownloadLink dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundDownloadLinkException);

            this.downloadLinkBrokerMock.Setup(broker =>
                broker.TakeAsync(inputId))
                    .ReturnsAsync(expiredDownloadLink);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            Func<Task<DownloadLink>> redeemTask = async () =>
                await this.downloadLinkService.RedeemDownloadLinkAsync(inputId);

            // then
            await redeemTask.Should()
                .ThrowAsync<DownloadLinkDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(
                        expectedDownloadLinkDependencyValidationException));

            // Auch der abgelaufene Link ist danach verbraucht: genommen wird er, bevor
            // die Zeit geprueft wird.
            this.downloadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(inputId),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDownloadLinkDependencyValidationException))),
                        Times.Once);

            this.downloadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
