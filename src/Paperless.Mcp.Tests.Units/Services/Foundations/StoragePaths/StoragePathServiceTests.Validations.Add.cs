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
        public async Task ShouldThrowValidationExceptionOnAddIfStoragePathIsNullAndLogItAsync()
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
            Func<Task<StoragePath>> addStoragePathTask = async () =>
                await this.storagePathService.AddStoragePathAsync(nullStoragePath!);

            // then
            await addStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnAddIfStoragePathNameIsInvalidAndLogItAsync(
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
            Func<Task<StoragePath>> addStoragePathTask = async () =>
                await this.storagePathService.AddStoragePathAsync(invalidStoragePath);

            // then
            await addStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnAddIfPathIsInvalidAndLogItAsync(
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
            Func<Task<StoragePath>> addStoragePathTask = async () =>
                await this.storagePathService.AddStoragePathAsync(invalidStoragePath);

            // then
            await addStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfStoragePathNameExceedsMaximumLengthAndLogItAsync()
        {
            // given
            StoragePath invalidStoragePath = CreateRandomStoragePath();
            invalidStoragePath.Name = new string('x', count: 129);

            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            invalidStoragePathException.UpsertDataList(
                key: nameof(StoragePath.Name),
                value: "Text exceeds the maximum length of 128 characters");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            // when
            Func<Task<StoragePath>> addStoragePathTask = async () =>
                await this.storagePathService.AddStoragePathAsync(invalidStoragePath);

            // then
            await addStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfMatchingAlgorithmIsUndefinedAndLogItAsync()
        {
            // given
            StoragePath invalidStoragePath = CreateRandomStoragePath();
            invalidStoragePath.MatchingAlgorithm = (StoragePathMatchingAlgorithm)99;

            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            invalidStoragePathException.UpsertDataList(
                key: nameof(StoragePath.MatchingAlgorithm),
                value: "Value is not a defined matching algorithm");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            // when
            Func<Task<StoragePath>> addStoragePathTask = async () =>
                await this.storagePathService.AddStoragePathAsync(invalidStoragePath);

            // then
            await addStoragePathTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostStoragePathAsync(It.IsAny<StoragePath>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
