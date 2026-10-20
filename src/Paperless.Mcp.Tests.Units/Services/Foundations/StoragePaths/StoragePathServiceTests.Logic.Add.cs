using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        public async Task ShouldAddStoragePathAsync()
        {
            // given
            StoragePath randomStoragePath = CreateRandomStoragePath();
            StoragePath inputStoragePath = randomStoragePath;
            StoragePath addedStoragePath = inputStoragePath.DeepClone();
            StoragePath expectedStoragePath = addedStoragePath.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostStoragePathAsync(inputStoragePath))
                    .ReturnsAsync(addedStoragePath);

            // when
            StoragePath actualStoragePath = await this.storagePathService.AddStoragePathAsync(inputStoragePath);

            // then
            actualStoragePath.Should().BeEquivalentTo(expectedStoragePath);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostStoragePathAsync(inputStoragePath),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
