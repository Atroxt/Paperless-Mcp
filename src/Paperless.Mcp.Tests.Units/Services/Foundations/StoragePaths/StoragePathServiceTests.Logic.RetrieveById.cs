using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveStoragePathByIdAsync()
        {
            // given
            StoragePath randomStoragePath = CreateRandomStoragePath();
            int inputStoragePathId = randomStoragePath.Id;
            StoragePath storedStoragePath = randomStoragePath;
            StoragePath expectedStoragePath = storedStoragePath.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStoragePathByIdAsync(inputStoragePathId))
                    .ReturnsAsync(storedStoragePath);

            // when
            StoragePath actualStoragePath = await this.storagePathService.RetrieveStoragePathByIdAsync(inputStoragePathId);

            // then
            actualStoragePath.Should().BeEquivalentTo(expectedStoragePath);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStoragePathByIdAsync(inputStoragePathId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
