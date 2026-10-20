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
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageIsInvalidAndLogItAsync(
            int invalidPage)
        {
            // given
            int validPageSize = GetRandomNumber();

            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            invalidStoragePathException.UpsertDataList(
                key: "Page",
                value: "Value must be greater than zero");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            // when
            Func<Task<StoragePathPage>> retrieveStoragePathPageTask = async () =>
                await this.storagePathService.RetrieveStoragePathPageAsync(invalidPage, validPageSize);

            // then
            await retrieveStoragePathPageTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStoragePathPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageSizeIsInvalidAndLogItAsync(
            int invalidPageSize)
        {
            // given
            int validPage = GetRandomNumber();

            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            invalidStoragePathException.UpsertDataList(
                key: "PageSize",
                value: "Value must be greater than zero");

            var expectedStoragePathValidationException =
                new StoragePathValidationException(
                    message: "StoragePath validation error occurred, fix the errors and try again.",
                    innerException: invalidStoragePathException);

            // when
            Func<Task<StoragePathPage>> retrieveStoragePathPageTask = async () =>
                await this.storagePathService.RetrieveStoragePathPageAsync(validPage, invalidPageSize);

            // then
            await retrieveStoragePathPageTask.Should()
                .ThrowAsync<StoragePathValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStoragePathValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStoragePathValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStoragePathPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
