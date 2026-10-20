using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.StoragePaths
{
    public partial class StoragePathServiceTests
    {
        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRemoveByIdIfIdIsInvalidAndLogItAsync(
            int invalidStoragePathId)
        {
            // given
            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            invalidStoragePathException.UpsertDataList(
                key: nameof(StoragePath.Id),
                value: "Value must be greater than zero");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            // when
            Func<Task> removeStoragePathByIdTask = async () =>
                await this.storagePathService.RemoveStoragePathByIdAsync(invalidStoragePathId);

            // then
            await removeStoragePathByIdTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
