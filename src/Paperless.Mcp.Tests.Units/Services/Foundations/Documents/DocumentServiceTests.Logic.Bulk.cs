using Moq;
using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldModifyDocumentsCorrespondentAsync()
        {
            // given
            List<int> randomDocumentIds = [GetRandomNumber(), GetRandomNumber()];
            int randomCorrespondentId = GetRandomNumber();

            // when
            await this.documentService.ModifyDocumentsCorrespondentAsync(
                randomDocumentIds, randomCorrespondentId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.Is<PaperlessBulkEdit>(bulkEdit =>
                    bulkEdit.Method == PaperlessBulkEdit.SetCorrespondentMethod
                    && bulkEdit.Documents == randomDocumentIds
                    && (int?)bulkEdit.Parameters[PaperlessBulkEdit.CorrespondentParameter]
                        == randomCorrespondentId)),
                            Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Der Fall, den es ohne Sammeloperation bisher nicht gab: eine Zuordnung leeren.
        // null muss als null hinausgehen, sonst bleibt der Korrespondent stehen.
        [TestMethod]
        public async Task ShouldClearCorrespondentOnModifyDocumentsCorrespondentAsync()
        {
            // given
            List<int> randomDocumentIds = [GetRandomNumber()];

            // when
            await this.documentService.ModifyDocumentsCorrespondentAsync(
                randomDocumentIds, correspondentId: null);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.Is<PaperlessBulkEdit>(bulkEdit =>
                    bulkEdit.Method == PaperlessBulkEdit.SetCorrespondentMethod
                    && bulkEdit.Parameters.ContainsKey(PaperlessBulkEdit.CorrespondentParameter)
                    && bulkEdit.Parameters[PaperlessBulkEdit.CorrespondentParameter] == null)),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldModifyDocumentsDocumentTypeAsync()
        {
            // given
            List<int> randomDocumentIds = [GetRandomNumber()];
            int randomDocumentTypeId = GetRandomNumber();

            // when
            await this.documentService.ModifyDocumentsDocumentTypeAsync(
                randomDocumentIds, randomDocumentTypeId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.Is<PaperlessBulkEdit>(bulkEdit =>
                    bulkEdit.Method == PaperlessBulkEdit.SetDocumentTypeMethod
                    && (int?)bulkEdit.Parameters[PaperlessBulkEdit.DocumentTypeParameter]
                        == randomDocumentTypeId)),
                            Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldModifyDocumentsStoragePathAsync()
        {
            // given
            List<int> randomDocumentIds = [GetRandomNumber()];
            int randomStoragePathId = GetRandomNumber();

            // when
            await this.documentService.ModifyDocumentsStoragePathAsync(
                randomDocumentIds, randomStoragePathId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.Is<PaperlessBulkEdit>(bulkEdit =>
                    bulkEdit.Method == PaperlessBulkEdit.SetStoragePathMethod
                    && (int?)bulkEdit.Parameters[PaperlessBulkEdit.StoragePathParameter]
                        == randomStoragePathId)),
                            Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // modify_tags verlangt beide Listen, auch die leere -- an der Instanz geprueft:
        // fehlt remove_tags, antwortet die API mit 400.
        [TestMethod]
        public async Task ShouldModifyDocumentsTagsAsync()
        {
            // given
            List<int> randomDocumentIds = [GetRandomNumber()];
            List<int> randomAddTagIds = [GetRandomNumber()];
            List<int> randomRemoveTagIds = [];

            // when
            await this.documentService.ModifyDocumentsTagsAsync(
                randomDocumentIds, randomAddTagIds, randomRemoveTagIds);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.Is<PaperlessBulkEdit>(bulkEdit =>
                    bulkEdit.Method == PaperlessBulkEdit.ModifyTagsMethod
                    && bulkEdit.Parameters[PaperlessBulkEdit.AddTagsParameter] == randomAddTagIds
                    && bulkEdit.Parameters[PaperlessBulkEdit.RemoveTagsParameter] == randomRemoveTagIds)),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldRemoveDocumentsAsync()
        {
            // given
            List<int> randomDocumentIds = [GetRandomNumber(), GetRandomNumber()];

            // when
            await this.documentService.RemoveDocumentsAsync(randomDocumentIds);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.PostBulkEditAsync(It.Is<PaperlessBulkEdit>(bulkEdit =>
                    bulkEdit.Method == PaperlessBulkEdit.DeleteMethod
                    && bulkEdit.Documents == randomDocumentIds
                    && bulkEdit.Parameters.Count == 0)),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
