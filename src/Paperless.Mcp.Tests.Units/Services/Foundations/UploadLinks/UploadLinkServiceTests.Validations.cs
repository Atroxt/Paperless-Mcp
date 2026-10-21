using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.UploadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.UploadLinks
{
    public partial class UploadLinkServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnIssueIfUploadLinkIsNullAndLogItAsync()
        {
            // given
            UploadLink nullUploadLink = null!;

            var nullUploadLinkException =
                new NullUploadLinkException(message: "UploadLink is null.");

            var expectedUploadLinkValidationException =
                new UploadLinkValidationException(
                    message: "UploadLink validation error occurred, fix the errors and try again.",
                    innerException: nullUploadLinkException);

            // when
            Func<Task> issueAction = async () =>
                await this.uploadLinkService.IssueUploadLinkAsync(nullUploadLink);

            // then
            await issueAction.Should().ThrowAsync<UploadLinkValidationException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedUploadLinkValidationException))),
                        Times.Once);

            this.uploadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnIssueIfTextsAreInvalidAndLogItAsync(
            string invalidText)
        {
            // given
            UploadLink invalidUploadLink = CreateRandomUploadLink(default);
            invalidUploadLink.FileName = invalidText;
            invalidUploadLink.Token = invalidText;

            var invalidUploadLinkException =
                new InvalidUploadLinkException(
                    message: "UploadLink is invalid, fix the errors and try again.");

            invalidUploadLinkException.AddData(
                key: nameof(UploadLink.FileName),
                values: "Text is required");

            invalidUploadLinkException.AddData(
                key: nameof(UploadLink.Token),
                values: "Text is required");

            var expectedUploadLinkValidationException =
                new UploadLinkValidationException(
                    message: "UploadLink validation error occurred, fix the errors and try again.",
                    innerException: invalidUploadLinkException);

            // when
            Func<Task> issueAction = async () =>
                await this.uploadLinkService.IssueUploadLinkAsync(invalidUploadLink);

            // then
            await issueAction.Should().ThrowAsync<UploadLinkValidationException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedUploadLinkValidationException))),
                        Times.Once);

            this.uploadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnIssueIfIdsAreInvalidAndLogItAsync(
            int invalidNumber)
        {
            // given
            UploadLink invalidUploadLink = CreateRandomUploadLink(default);
            invalidUploadLink.CorrespondentId = invalidNumber;
            invalidUploadLink.DocumentTypeId = invalidNumber;
            invalidUploadLink.StoragePathId = invalidNumber;

            var invalidUploadLinkException =
                new InvalidUploadLinkException(
                    message: "UploadLink is invalid, fix the errors and try again.");

            invalidUploadLinkException.AddData(
                key: nameof(UploadLink.CorrespondentId),
                values: "Value must be greater than zero");

            invalidUploadLinkException.AddData(
                key: nameof(UploadLink.DocumentTypeId),
                values: "Value must be greater than zero");

            invalidUploadLinkException.AddData(
                key: nameof(UploadLink.StoragePathId),
                values: "Value must be greater than zero");

            var expectedUploadLinkValidationException =
                new UploadLinkValidationException(
                    message: "UploadLink validation error occurred, fix the errors and try again.",
                    innerException: invalidUploadLinkException);

            // when
            Func<Task> issueAction = async () =>
                await this.uploadLinkService.IssueUploadLinkAsync(invalidUploadLink);

            // then
            await issueAction.Should().ThrowAsync<UploadLinkValidationException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedUploadLinkValidationException))),
                        Times.Once);

            this.uploadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRedeemIfLinkIsUnknownAndLogItAsync()
        {
            // given
            string unknownId = GetRandomId();

            this.uploadLinkBrokerMock.Setup(broker =>
                broker.TakeAsync(unknownId))
                    .ReturnsAsync((UploadLink?)null);

            var notFoundUploadLinkException =
                new NotFoundUploadLinkException(
                    message: "Upload link is unknown or was already used.");

            var expectedException =
                new UploadLinkDependencyValidationException(
                    message: "UploadLink dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundUploadLinkException);

            // when
            Func<Task> redeemAction = async () =>
                await this.uploadLinkService.RedeemUploadLinkAsync(unknownId);

            // then
            await redeemAction.Should().ThrowAsync<UploadLinkDependencyValidationException>();

            this.uploadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(unknownId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedException))),
                    Times.Once);

            this.uploadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRedeemIfLinkIsExpiredAndLogItAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            UploadLink expiredUploadLink = CreateRandomUploadLink(randomDateTimeOffset);
            string inputId = expiredUploadLink.Id;

            this.uploadLinkBrokerMock.Setup(broker =>
                broker.TakeAsync(inputId))
                    .ReturnsAsync(expiredUploadLink);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            var notFoundUploadLinkException =
                new NotFoundUploadLinkException(message: "Upload link has expired.");

            var expectedException =
                new UploadLinkDependencyValidationException(
                    message: "UploadLink dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundUploadLinkException);

            // when
            Func<Task> redeemAction = async () =>
                await this.uploadLinkService.RedeemUploadLinkAsync(inputId);

            // then
            await redeemAction.Should().ThrowAsync<UploadLinkDependencyValidationException>();

            this.uploadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(inputId),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedException))),
                    Times.Once);

            this.uploadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
