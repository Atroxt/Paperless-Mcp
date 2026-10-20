using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        public async Task ShouldRemoveStoragePathByIdAsync()
        {
            // given
            StoragePath randomStoragePath = CreateRandomStoragePath();
            int inputStoragePathId = randomStoragePath.Id;

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteStoragePathByIdAsync(inputStoragePathId))
                    .Returns(ValueTask.CompletedTask);

            // when
            await this.storagePathService.RemoveStoragePathByIdAsync(inputStoragePathId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(inputStoragePathId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
