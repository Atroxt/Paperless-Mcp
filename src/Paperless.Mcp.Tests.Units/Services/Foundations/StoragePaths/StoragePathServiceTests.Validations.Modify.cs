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
        public async Task ShouldThrowValidationExceptionOnModifyIfStoragePathIsNullAndLogItAsync()
        {
            // given
            StoragePath? nullStoragePath = null;

            var nullStoragePathException =
                new NullStoragePathException(message: "StoragePath is null.");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: nullStoragePathException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(nullStoragePath!);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnModifyIfIdIsInvalidAndLogItAsync(
            int invalidId)
        {
            // given
            StoragePath invalidStoragePath = CreateRandomStoragePath();
            invalidStoragePath.Id = invalidId;

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
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(invalidStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnModifyIfStoragePathNameIsInvalidAndLogItAsync(
            string? invalidName)
        {
            // given
            StoragePath invalidStoragePath = CreateRandomStoragePath();
            invalidStoragePath.Name = invalidName!;

            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            invalidStoragePathException.UpsertDataList(
                key: nameof(StoragePath.Name),
                value: "Text is required");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(invalidStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnModifyIfPathIsInvalidAndLogItAsync(
            string? invalidPath)
        {
            // given
            StoragePath invalidStoragePath = CreateRandomStoragePath();
            invalidStoragePath.Path = invalidPath!;

            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            invalidStoragePathException.UpsertDataList(
                key: nameof(StoragePath.Path),
                value: "Text is required");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            // when
            Func<Task<StoragePath>> modifyStoragePathTask = async () =>
                await this.storagePathService.ModifyStoragePathAsync(invalidStoragePath);

            // then
            await modifyStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
