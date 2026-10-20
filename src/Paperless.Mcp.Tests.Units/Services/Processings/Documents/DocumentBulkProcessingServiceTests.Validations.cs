using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Documents
{
    public partial class DocumentBulkProcessingServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkUpdateIfUpdateIsNullAsync()
        {
            // when
            Func<Task> bulkUpdateTask = async () =>
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    null!, confirm: false);

            // then
            await bulkUpdateTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        // Eine Sammeloperation ohne Aenderung waere der gefaehrlichste Leerlauf: der
        // Bericht nennt betroffene Dokumente, geschehen waere nichts.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkUpdateIfNothingChangesAsync()
        {
            // given
            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = [GetRandomNumber()],
            };

            // when
            Func<Task> bulkUpdateTask = async () =>
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm: false);

            // then
            await bulkUpdateTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkUpdateIfDocumentIdsAreEmptyAsync()
        {
            // given
            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = [],
                Correspondent = new DocumentReferenceUpdate { Id = GetRandomNumber() },
            };

            // when
            Func<Task> bulkUpdateTask = async () =>
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm: false);

            // then
            await bulkUpdateTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        // Setzen und Leeren zugleich ist keine Absicht, sondern ein Irrtum -- und einer,
        // der sonst still zum Leeren fuehrte.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkUpdateIfReferenceIsAmbiguousAsync()
        {
            // given
            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = [GetRandomNumber()],

                Correspondent = new DocumentReferenceUpdate
                {
                    Id = GetRandomNumber(),
                    Clear = true,
                },
            };

            // when
            Func<Task> bulkUpdateTask = async () =>
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm: false);

            // then
            await bulkUpdateTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkUpdateIfTagIsAddedAndRemovedAsync()
        {
            // given
            var documentBulkUpdate = new DocumentBulkUpdate
            {
                DocumentIds = [GetRandomNumber()],
                AddTagIds = [7, 8],
                RemoveTagIds = [8],
            };

            // when
            Func<Task> bulkUpdateTask = async () =>
                await this.documentBulkProcessingService.BulkUpdateDocumentsAsync(
                    documentBulkUpdate, confirm: false);

            // then
            await bulkUpdateTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnBulkDeleteIfDocumentIdsAreEmptyAsync()
        {
            // when
            Func<Task> bulkRemoveTask = async () =>
                await this.documentBulkProcessingService.BulkRemoveDocumentsAsync(
                    [], confirm: true);

            // then
            await bulkRemoveTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.VerifyNoOtherCalls();
        }
    }
}
