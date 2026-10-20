using System.Text;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldAddDocumentFileAsync()
        {
            // given
            var documentUpload = new DocumentUpload
            {
                FileName = "rechnung.pdf",
                Content = Encoding.UTF8.GetBytes("inhalt"),
                Title = "Rechnung",
            };

            string expectedTaskId = Guid.NewGuid().ToString();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentFileAsync(It.IsAny<PaperlessDocumentForm>()))
                    .ReturnsAsync(expectedTaskId);

            // when
            string actualTaskId =
                await this.documentService.AddDocumentFileAsync(documentUpload);

            // then
            // Paperless antwortet mit der Auftragskennung, nicht mit einem Dokument.
            actualTaskId.Should().Be(expectedTaskId);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentFileAsync(It.Is<PaperlessDocumentForm>(form =>
                    form.FileName == "rechnung.pdf"
                    && form.Title == "Rechnung"
                    && form.Document.Length > 0)),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldLeaveUnsetUploadFieldsNullAsync()
        {
            // given
            var documentUpload = new DocumentUpload
            {
                FileName = "ohne-angaben.txt",
                Content = Encoding.UTF8.GetBytes("inhalt"),
            };

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentFileAsync(It.IsAny<PaperlessDocumentForm>()))
                    .ReturnsAsync(Guid.NewGuid().ToString());

            // when
            await this.documentService.AddDocumentFileAsync(documentUpload);

            // then
            // Nicht gesetzte Angaben bleiben null; der Broker laesst sie dann im
            // Formular weg, statt sie leer zu senden.
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentFileAsync(It.Is<PaperlessDocumentForm>(form =>
                    form.Title == null
                    && form.Correspondent == null
                    && form.DocumentType == null
                    && form.StoragePath == null
                    && form.Created == null)),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldSendIdsAndDateAsTextOnUploadAsync()
        {
            // given
            var documentUpload = new DocumentUpload
            {
                FileName = "mit-angaben.txt",
                Content = Encoding.UTF8.GetBytes("inhalt"),
                CorrespondentId = 5,
                DocumentTypeId = 6,
                StoragePathId = 7,
                Created = new DateOnly(2026, 2, 3),
            };

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentFileAsync(It.IsAny<PaperlessDocumentForm>()))
                    .ReturnsAsync(Guid.NewGuid().ToString());

            // when
            await this.documentService.AddDocumentFileAsync(documentUpload);

            // then
            // Multipart kennt keine Typen -- Zahlen und Datum gehen als Text hinaus.
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentFileAsync(It.Is<PaperlessDocumentForm>(form =>
                    form.Correspondent == "5"
                    && form.DocumentType == "6"
                    && form.StoragePath == "7"
                    && form.Created == "2026-02-03")),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldRetrieveDocumentThumbnailAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
            byte[] storedContent = [1, 2, 3, 4];

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentThumbnailAsync(someDocumentId))
                    .ReturnsAsync(storedContent);

            // when
            DocumentThumbnail actualThumbnail =
                await this.documentService.RetrieveDocumentThumbnailAsync(someDocumentId);

            // then
            actualThumbnail.DocumentId.Should().Be(someDocumentId);
            actualThumbnail.Content.Should().BeEquivalentTo(storedContent);
            actualThumbnail.MimeType.Should().Be("image/webp");

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentThumbnailAsync(someDocumentId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
