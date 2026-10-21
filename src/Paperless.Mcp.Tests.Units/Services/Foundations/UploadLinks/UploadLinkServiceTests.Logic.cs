using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.UploadLinks;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.UploadLinks
{
    public partial class UploadLinkServiceTests
    {
        [TestMethod]
        public async Task ShouldIssueUploadLinkAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            string randomId = GetRandomId();
            UploadLink inputUploadLink = CreateRandomUploadLink(default);
            inputUploadLink.Id = string.Empty;

            UploadLink expectedUploadLink = inputUploadLink.DeepClone();
            expectedUploadLink.Id = randomId;
            expectedUploadLink.ExpiresAt = randomDateTimeOffset.AddSeconds(LifetimeSeconds);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.uploadLinkBrokerMock.Setup(broker =>
                broker.GenerateIdAsync())
                    .ReturnsAsync(randomId);

            // when
            UploadLink actualUploadLink =
                await this.uploadLinkService.IssueUploadLinkAsync(inputUploadLink);

            // then
            actualUploadLink.Should().BeEquivalentTo(expectedUploadLink);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.uploadLinkBrokerMock.Verify(broker =>
                broker.RemoveExpiredAsync(randomDateTimeOffset),
                    Times.Once);

            this.uploadLinkBrokerMock.Verify(broker =>
                broker.GenerateIdAsync(),
                    Times.Once);

            this.uploadLinkBrokerMock.Verify(broker =>
                broker.StoreAsync(It.Is<UploadLink>(uploadLink =>
                    uploadLink.Id == randomId
                    && uploadLink.Token == expectedUploadLink.Token
                    && uploadLink.ExpiresAt == expectedUploadLink.ExpiresAt)),
                        Times.Once);

            this.uploadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldIssueUploadLinkWithoutOptionalMetadataAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();
            UploadLink inputUploadLink = CreateRandomUploadLink(default);
            inputUploadLink.Title = null;
            inputUploadLink.CorrespondentId = null;
            inputUploadLink.DocumentTypeId = null;
            inputUploadLink.StoragePathId = null;
            inputUploadLink.Created = null;

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            this.uploadLinkBrokerMock.Setup(broker =>
                broker.GenerateIdAsync())
                    .ReturnsAsync(GetRandomId());

            // when
            UploadLink actualUploadLink =
                await this.uploadLinkService.IssueUploadLinkAsync(inputUploadLink);

            // then
            actualUploadLink.CorrespondentId.Should().BeNull();
            actualUploadLink.Title.Should().BeNull();

            this.uploadLinkBrokerMock.Verify(broker =>
                broker.StoreAsync(It.IsAny<UploadLink>()),
                    Times.Once);

            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldRedeemUploadLinkAsync()
        {
            // given
            DateTimeOffset randomDateTimeOffset = GetRandomDateTimeOffset();

            UploadLink storedUploadLink =
                CreateRandomUploadLink(randomDateTimeOffset.AddSeconds(1));

            UploadLink expectedUploadLink = storedUploadLink.DeepClone();
            string inputId = storedUploadLink.Id;

            this.uploadLinkBrokerMock.Setup(broker =>
                broker.TakeAsync(inputId))
                    .ReturnsAsync(storedUploadLink);

            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(randomDateTimeOffset);

            // when
            UploadLink actualUploadLink =
                await this.uploadLinkService.RedeemUploadLinkAsync(inputId);

            // then
            actualUploadLink.Should().BeEquivalentTo(expectedUploadLink);

            this.uploadLinkBrokerMock.Verify(broker =>
                broker.TakeAsync(inputId),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.uploadLinkBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
