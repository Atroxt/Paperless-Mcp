using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Documents
{
    public partial class DocumentBulkProcessingServiceTests
    {
        // Der Kern des Guards: ohne Bestaetigung wird gelesen, aber nicht geschrieben.
        [TestMethod]
        public async Task ShouldNotChangeAnythingOnBulkUpdateDryRunAsync()
        {
            // given
            Document firstDocument = CreateDocument(id: 11);
            Document secondDocument = CreateDocument(id: 12);
            SetUpFoundDocuments(firstDocument, secondDocument);

            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = [11, 12],
                Correspondent = new DocumentReferenceUpdate { Id = 5 },
            };

            // when
            DocumentBulkPlan actualPlan =
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm: false);

            // then
            actualPlan.Applied.Should().BeFalse();
            actualPlan.AffectedDocumentIds.Should().BeEquivalentTo([11, 12]);
            actualPlan.UnknownDocumentIds.Should().BeEmpty();

            actualPlan.AffectedTitles.Should().BeEquivalentTo(
                [firstDocument.Title, secondDocument.Title]);

            actualPlan.Changes.Should().ContainSingle()
                .Which.Should().Be("Correspondent set to 5");

            actualPlan.Summary.Should().Contain("Dry run");

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.ModifyDocumentsCorrespondentAsync(
                    It.IsAny<List<int>>(), It.IsAny<int?>()),
                        Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldApplyBulkUpdateWhenConfirmedAsync()
        {
            // given
            SetUpFoundDocuments(CreateDocument(id: 11), CreateDocument(id: 12));

            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = [11, 12],
                Correspondent = new DocumentReferenceUpdate { Id = 5 },
                AddTagIds = [7],
            };

            int[] expectedDocumentIds = [11, 12];
            int[] expectedAddTagIds = [7];

            // when
            DocumentBulkPlan actualPlan =
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm: true);

            // then
            actualPlan.Applied.Should().BeTrue();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.ModifyDocumentsCorrespondentAsync(
                    It.Is<List<int>>(ids => ids.SequenceEqual(expectedDocumentIds)), 5),
                        Times.Once);

            this.documentServiceMock.Verify(service =>
                service.ModifyDocumentsTagsAsync(
                    It.Is<List<int>>(ids => ids.SequenceEqual(expectedDocumentIds)),
                    It.Is<List<int>>(ids => ids.SequenceEqual(expectedAddTagIds)),
                    It.Is<List<int>>(ids => ids.Count == 0)),
                        Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        // Clear und Id sind zwei verschiedene Absichten. Clear muss als null ankommen.
        [TestMethod]
        public async Task ShouldClearReferenceWhenConfirmedAsync()
        {
            // given
            SetUpFoundDocuments(CreateDocument(id: 11));

            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = [11],
                Correspondent = new DocumentReferenceUpdate { Clear = true },
            };

            // when
            DocumentBulkPlan actualPlan =
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm: true);

            // then
            actualPlan.Changes.Should().ContainSingle()
                .Which.Should().Be("Correspondent cleared");

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.ModifyDocumentsCorrespondentAsync(
                    It.IsAny<List<int>>(), null),
                        Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        // Paperless laesst unbekannte Ids stillschweigend weg. Der Bericht darf das nicht
        // auch tun, sonst geht ein Tippfehler als Erfolg durch.
        [TestMethod]
        public async Task ShouldReportUnknownDocumentIdsAsync()
        {
            // given
            SetUpFoundDocuments(CreateDocument(id: 11));

            // when
            DocumentBulkPlan actualPlan =
                await this.documentBulkProcessingService.BulkRemoveDocumentsAsync(
                    [11, 9999], confirm: false);

            // then
            actualPlan.AffectedDocumentIds.Should().BeEquivalentTo([11]);
            actualPlan.UnknownDocumentIds.Should().BeEquivalentTo([9999]);
            actualPlan.Summary.Should().Contain("9999");

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RemoveDocumentsAsync(It.IsAny<List<int>>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        // Nur die tatsaechlich vorhandenen gehen hinaus: was die API ohnehin ignorieren
        // wuerde, soll gar nicht erst Teil des Auftrags sein.
        [TestMethod]
        public async Task ShouldDeleteOnlyExistingDocumentsWhenConfirmedAsync()
        {
            // given
            SetUpFoundDocuments(CreateDocument(id: 11));
            int[] expectedDocumentIds = [11];

            // when
            DocumentBulkPlan actualPlan =
                await this.documentBulkProcessingService.BulkRemoveDocumentsAsync(
                    [11, 9999], confirm: true);

            // then
            actualPlan.Applied.Should().BeTrue();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RemoveDocumentsAsync(
                    It.Is<List<int>>(ids => ids.SequenceEqual(expectedDocumentIds))),
                        Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldNotApplyWhenNoRequestedDocumentExistsAsync()
        {
            // given
            SetUpFoundDocuments();

            // when
            DocumentBulkPlan actualPlan =
                await this.documentBulkProcessingService.BulkRemoveDocumentsAsync(
                    [9998, 9999], confirm: true);

            // then
            actualPlan.Applied.Should().BeFalse();
            actualPlan.Summary.Should().Contain("none of the requested documents exist");

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RemoveDocumentsAsync(It.IsAny<List<int>>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        // Die Leseabfrage muss genau die angefragten Ids einschliessen, sonst zaehlt der
        // Bericht etwas anderes als das, was geaendert wird.
        [TestMethod]
        public async Task ShouldReadExactlyTheRequestedDocumentsAsync()
        {
            // given
            SetUpFoundDocuments(CreateDocument(id: 11), CreateDocument(id: 12));
            int[] expectedDocumentIds = [11, 12];

            // when
            await this.documentBulkProcessingService.BulkRemoveDocumentsAsync(
                [11, 12], confirm: false);

            // then
            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.Is<DocumentSearch>(search =>
                    search.DocumentIds.SequenceEqual(expectedDocumentIds)
                    && search.PageSize == 2)),
                        Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }
    }
}
