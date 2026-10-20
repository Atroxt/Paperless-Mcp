using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldModifyDocumentAsync()
        {
            // given
            Document randomDocument = CreateRandomDocument();
            Document modifiedDocument = randomDocument;
            Document expectedDocument = modifiedDocument.DeepClone();

            var inputUpdate = new DocumentUpdate
            {
                DocumentId = randomDocument.Id,
                Title = "Neuer Titel",
            };

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(
                    randomDocument.Id,
                    It.IsAny<PaperlessDocumentUpdate>()))
                        .ReturnsAsync(modifiedDocument);

            // when
            Document actualDocument =
                await this.documentService.ModifyDocumentAsync(inputUpdate);

            // then
            actualDocument.Should().BeEquivalentTo(expectedDocument);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(
                    randomDocument.Id,
                    It.Is<PaperlessDocumentUpdate>(wire => wire.Title == "Neuer Titel")),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapBusinessNamesToWireNamesOnModifyAsync()
        {
            // given
            Document someDocument = CreateRandomDocument();

            var inputUpdate = new DocumentUpdate
            {
                DocumentId = someDocument.Id,
                CorrespondentId = 11,
                DocumentTypeId = 12,
                StoragePathId = 13,
                TagIds = [21, 22],
                Created = new DateOnly(2026, 3, 4),
                ArchiveSerialNumber = 777,
                CustomFields = [new DocumentCustomFieldValue { Field = 31, Value = "EUR9.99" }],
            };

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ReturnsAsync(someDocument);

            // when
            await this.documentService.ModifyDocumentAsync(inputUpdate);

            // then
            // CorrespondentId wird zu Correspondent, TagIds zu Tags -- Paperless kennt
            // die Id-Suffixe nicht.
            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(
                    someDocument.Id,
                    It.Is<PaperlessDocumentUpdate>(wire =>
                        wire.Correspondent == 11
                        && wire.DocumentType == 12
                        && wire.StoragePath == 13
                        && wire.Tags!.Count == 2
                        && wire.Created == new DateOnly(2026, 3, 4)
                        && wire.ArchiveSerialNumber == 777
                        && wire.CustomFields!.Count == 1
                        && wire.Title == null)),
                            Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldForwardEmptyTagListToRemoveAllTagsAsync()
        {
            // given
            Document someDocument = CreateRandomDocument();

            var inputUpdate = new DocumentUpdate
            {
                DocumentId = someDocument.Id,
                TagIds = [],
            };

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ReturnsAsync(someDocument);

            // when
            await this.documentService.ModifyDocumentAsync(inputUpdate);

            // then
            // Leere Liste heisst "alle entfernen" und muss deshalb gesendet werden,
            // nicht weggelassen.
            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(
                    someDocument.Id,
                    It.Is<PaperlessDocumentUpdate>(wire =>
                        wire.Tags != null && wire.Tags.Count == 0)),
                            Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
