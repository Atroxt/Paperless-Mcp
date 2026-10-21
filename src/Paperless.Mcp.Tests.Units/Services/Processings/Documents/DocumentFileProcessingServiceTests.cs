using System.Text;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Brokers.Files;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Processings.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Documents
{
    [TestClass]
    public class DocumentFileProcessingServiceTests
    {
        private readonly Mock<IDocumentService> documentServiceMock;
        private readonly Mock<IFileBroker> fileBrokerMock;

        public DocumentFileProcessingServiceTests()
        {
            this.documentServiceMock = new Mock<IDocumentService>();
            this.fileBrokerMock = new Mock<IFileBroker>();
        }

        private IDocumentFileProcessingService CreateService(bool allowLocalFileAccess) =>
            new DocumentFileProcessingService(
                documentService: this.documentServiceMock.Object,
                fileBroker: this.fileBrokerMock.Object,
                configurations: new DocumentFileConfigurations
                {
                    AllowLocalFileAccess = allowLocalFileAccess,
                });

        // Der wichtigste Test dieser Schicht: ohne Erlaubnis wird das Dateisystem nicht
        // angefasst -- auch nicht lesend.
        [TestMethod]
        public async Task ShouldRefuseUploadWhenLocalFileAccessIsDisabledAsync()
        {
            // given
            IDocumentFileProcessingService service = CreateService(allowLocalFileAccess: false);

            // when
            Func<Task<string>> uploadTask = async () =>
                await service.UploadFromPathAsync(
                    @"C:\irgendwo\geheim.txt", null, null, null, null);

            // then
            await uploadTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.fileBrokerMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldRefuseDownloadWhenLocalFileAccessIsDisabledAsync()
        {
            // given
            IDocumentFileProcessingService service = CreateService(allowLocalFileAccess: false);

            // when
            Func<Task<long>> downloadTask = async () =>
                await service.DownloadToPathAsync(1, @"C:\irgendwo\ziel.pdf", original: false);

            // then
            await downloadTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.fileBrokerMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldUploadFromPathWhenAllowedAsync()
        {
            // given
            IDocumentFileProcessingService service = CreateService(allowLocalFileAccess: true);

            string filePath = CreateFilePath("rechnung.pdf");
            byte[] fileContent = Encoding.UTF8.GetBytes("inhalt");
            string expectedTaskId = Guid.NewGuid().ToString();

            this.fileBrokerMock.Setup(broker => broker.ExistsAsync(filePath))
                .ReturnsAsync(true);

            using var fileStream = new MemoryStream(fileContent);

            this.fileBrokerMock.Setup(broker => broker.OpenReadAsync(filePath))
                .ReturnsAsync(fileStream);

            this.documentServiceMock.Setup(service =>
                service.AddDocumentFileAsync(It.IsAny<DocumentUpload>()))
                    .ReturnsAsync(expectedTaskId);

            // when
            string actualTaskId = await service.UploadFromPathAsync(
                filePath, title: "Rechnung", null, null, null);

            // then
            actualTaskId.Should().Be(expectedTaskId);

            // Der Dateiname kommt aus dem Pfad, nicht vom Aufrufer
            this.documentServiceMock.Verify(service =>
                service.AddDocumentFileAsync(It.Is<DocumentUpload>(upload =>
                    upload.FileName == "rechnung.pdf"
                    && upload.Title == "Rechnung"
                    && upload.ContentStream == fileStream)),
                        Times.Once);

            this.fileBrokerMock.Verify(broker => broker.ExistsAsync(filePath), Times.Once);
            this.fileBrokerMock.Verify(broker => broker.OpenReadAsync(filePath), Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportMissingFileOnUploadAsync()
        {
            // given
            IDocumentFileProcessingService service = CreateService(allowLocalFileAccess: true);
            string filePath = CreateFilePath("gibtsnicht.pdf");

            this.fileBrokerMock.Setup(broker => broker.ExistsAsync(filePath))
                .ReturnsAsync(false);

            // when
            Func<Task<string>> uploadTask = async () =>
                await service.UploadFromPathAsync(filePath, null, null, null, null);

            // then
            await uploadTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            // Nicht lesen, was es nicht gibt, und erst recht nichts hochladen
            this.fileBrokerMock.Verify(broker => broker.ExistsAsync(filePath), Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.OpenReadAsync(It.IsAny<string>()),
                    Times.Never);

            this.fileBrokerMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDownloadToPathWhenAllowedAsync()
        {
            // given
            IDocumentFileProcessingService service = CreateService(allowLocalFileAccess: true);

            string targetPath = CreateFilePath("ziel.pdf");
            byte[] fileContent = [1, 2, 3, 4, 5];

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentFileAsync(13, true))
                    .ReturnsAsync(fileContent);

            // when
            long actualBytes = await service.DownloadToPathAsync(13, targetPath, original: true);

            // then
            actualBytes.Should().Be(5);

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentFileAsync(13, true),
                    Times.Once);

            this.fileBrokerMock.Verify(broker =>
                broker.WriteAllBytesAsync(targetPath, fileContent),
                    Times.Once);

            this.fileBrokerMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public async Task ShouldThrowValidationExceptionIfPathIsInvalidAsync(string? invalidPath)
        {
            // given
            IDocumentFileProcessingService service = CreateService(allowLocalFileAccess: true);

            // when
            Func<Task<string>> uploadTask = async () =>
                await service.UploadFromPathAsync(invalidPath!, null, null, null, null);

            // then
            await uploadTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.fileBrokerMock.VerifyNoOtherCalls();
            this.documentServiceMock.VerifyNoOtherCalls();
        }

        // Pfade im Format des laufenden Systems: unter Linux ist \ kein Trennzeichen,
        // Path.GetFileName lieferte fuer "C:\ablage\x.pdf" sonst den ganzen String.
        private static string CreateFilePath(string fileName) =>
            Path.Combine(Path.GetTempPath(), "ablage", fileName);
    }
}
