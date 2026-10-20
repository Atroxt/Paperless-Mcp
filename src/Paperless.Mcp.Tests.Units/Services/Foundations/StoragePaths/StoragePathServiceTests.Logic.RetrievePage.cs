using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveStoragePathPageAsync()
        {
            // given
            int inputPage = GetRandomNumber();
            int inputPageSize = GetRandomNumber();
            StoragePathPage randomStoragePathPage = CreateRandomStoragePathPage();
            StoragePathPage storedStoragePathPage = randomStoragePathPage;
            StoragePathPage expectedStoragePathPage = storedStoragePathPage.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStoragePathPageAsync(inputPage, inputPageSize))
                    .ReturnsAsync(storedStoragePathPage);

            // when
            StoragePathPage actualStoragePathPage =
                await this.storagePathService.RetrieveStoragePathPageAsync(inputPage, inputPageSize);

            // then
            actualStoragePathPage.Should().BeEquivalentTo(expectedStoragePathPage);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStoragePathPageAsync(inputPage, inputPageSize),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
