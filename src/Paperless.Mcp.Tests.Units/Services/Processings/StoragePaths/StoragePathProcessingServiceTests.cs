using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using Paperless.Mcp.Models.Processings.StoragePaths;
using Paperless.Mcp.Models.Processings.StoragePaths.Exceptions;
using Paperless.Mcp.Services.Foundations.StoragePaths;
using Paperless.Mcp.Services.Processings.StoragePaths;

namespace Paperless.Mcp.Tests.Units.Services.Processings.StoragePaths
{
    [TestClass]
    public class StoragePathProcessingServiceTests
    {
        private readonly Mock<IStoragePathService> storagePathServiceMock;
        private readonly IStoragePathProcessingService storagePathProcessingService;

        public StoragePathProcessingServiceTests()
        {
            this.storagePathServiceMock = new Mock<IStoragePathService>();

            this.storagePathProcessingService =
                new StoragePathProcessingService(storagePathService: this.storagePathServiceMock.Object);
        }

        private static StoragePath CreateStoragePath(int documentCount) =>
            new() { Id = 7, Name = "Rechnungen", DocumentCount = documentCount };

        // Der wichtigste Test dieser Schicht: ohne Bestaetigung darf nichts verschwinden.
        [TestMethod]
        public async Task ShouldNotDeleteOnDryRunAsync()
        {
            // given
            StoragePath storedStoragePath = CreateStoragePath(documentCount: 143);

            this.storagePathServiceMock.Setup(service =>
                service.RetrieveStoragePathByIdAsync(storedStoragePath.Id))
                    .ReturnsAsync(storedStoragePath);

            // when
            StoragePathRemovalPlan actualPlan =
                await this.storagePathProcessingService.RemoveStoragePathByIdAsync(storedStoragePath.Id, confirm: false);

            // then
            actualPlan.Removed.Should().BeFalse();
            actualPlan.StoragePathId.Should().Be(storedStoragePath.Id);
            actualPlan.Name.Should().Be("Rechnungen");
            actualPlan.DocumentCount.Should().Be(143);
            actualPlan.Summary.Should().Contain("Dry run");
            actualPlan.Summary.Should().Contain("143");

            this.storagePathServiceMock.Verify(service =>
                service.RetrieveStoragePathByIdAsync(storedStoragePath.Id),
                    Times.Once);

            this.storagePathServiceMock.Verify(service =>
                service.RemoveStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.storagePathServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDeleteWhenConfirmedAsync()
        {
            // given
            StoragePath storedStoragePath = CreateStoragePath(documentCount: 2);

            this.storagePathServiceMock.Setup(service =>
                service.RetrieveStoragePathByIdAsync(storedStoragePath.Id))
                    .ReturnsAsync(storedStoragePath);

            // when
            StoragePathRemovalPlan actualPlan =
                await this.storagePathProcessingService.RemoveStoragePathByIdAsync(storedStoragePath.Id, confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();
            actualPlan.DocumentCount.Should().Be(2);
            actualPlan.Summary.Should().NotContain("Dry run");

            // Erst lesen, dann loeschen: der Bericht soll die Tragweite auch im
            // bestaetigten Fall nennen koennen.
            this.storagePathServiceMock.Verify(service =>
                service.RetrieveStoragePathByIdAsync(storedStoragePath.Id),
                    Times.Once);

            this.storagePathServiceMock.Verify(service =>
                service.RemoveStoragePathByIdAsync(storedStoragePath.Id),
                    Times.Once);

            this.storagePathServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportWhenNoDocumentCarriesTheStoragePathAsync()
        {
            // given
            StoragePath storedStoragePath = CreateStoragePath(documentCount: 0);

            this.storagePathServiceMock.Setup(service =>
                service.RetrieveStoragePathByIdAsync(storedStoragePath.Id))
                    .ReturnsAsync(storedStoragePath);

            // when
            StoragePathRemovalPlan actualPlan =
                await this.storagePathProcessingService.RemoveStoragePathByIdAsync(storedStoragePath.Id, confirm: false);

            // then
            actualPlan.Summary.Should().Contain("No document");

            this.storagePathServiceMock.Verify(service =>
                service.RetrieveStoragePathByIdAsync(storedStoragePath.Id),
                    Times.Once);

            this.storagePathServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task ShouldThrowValidationExceptionIfIdIsInvalidAsync(int invalidStoragePathId)
        {
            // when
            Func<Task<StoragePathRemovalPlan>> removeTask = async () =>
                await this.storagePathProcessingService.RemoveStoragePathByIdAsync(invalidStoragePathId, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<StoragePathProcessingValidationException>();

            // Eine ungueltige Id darf gar nicht erst beim Dienst ankommen
            this.storagePathServiceMock.Verify(service =>
                service.RetrieveStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.storagePathServiceMock.Verify(service =>
                service.RemoveStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.storagePathServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapNotFoundToDependencyValidationAsync()
        {
            // given
            var storagePathDependencyValidationException =
                new StoragePathDependencyValidationException(
                    message: "StoragePath dependency validation error occurred, fix the errors and try again.",
                    innerException: new NotFoundStoragePathException(
                        message: "StoragePath not found.",
                        innerException: new Exception()));

            this.storagePathServiceMock.Setup(service =>
                service.RetrieveStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(storagePathDependencyValidationException);

            // when
            Func<Task<StoragePathRemovalPlan>> removeTask = async () =>
                await this.storagePathProcessingService.RemoveStoragePathByIdAsync(99, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<StoragePathProcessingDependencyValidationException>();

            this.storagePathServiceMock.Verify(service =>
                service.RetrieveStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Once);

            // Gescheitertes Lesen heisst: nicht loeschen
            this.storagePathServiceMock.Verify(service =>
                service.RemoveStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.storagePathServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapUnexpectedErrorToProcessingServiceExceptionAsync()
        {
            // given
            this.storagePathServiceMock.Setup(service =>
                service.RetrieveStoragePathByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

            // when
            Func<Task<StoragePathRemovalPlan>> removeTask = async () =>
                await this.storagePathProcessingService.RemoveStoragePathByIdAsync(7, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<StoragePathProcessingServiceException>();

            this.storagePathServiceMock.Verify(service =>
                service.RetrieveStoragePathByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.storagePathServiceMock.VerifyNoOtherCalls();
        }
    }
}
