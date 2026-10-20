using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        public async Task ShouldModifyStoragePathAsync()
        {
            // given
            StoragePath randomStoragePath = CreateRandomStoragePath();
            StoragePath inputStoragePath = randomStoragePath;
            StoragePath modifiedStoragePath = inputStoragePath.DeepClone();
            StoragePath expectedStoragePath = modifiedStoragePath.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutStoragePathAsync(inputStoragePath))
                    .ReturnsAsync(modifiedStoragePath);

            // when
            StoragePath actualStoragePath = await this.storagePathService.ModifyStoragePathAsync(inputStoragePath);

            // then
            actualStoragePath.Should().BeEquivalentTo(expectedStoragePath);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(inputStoragePath),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
