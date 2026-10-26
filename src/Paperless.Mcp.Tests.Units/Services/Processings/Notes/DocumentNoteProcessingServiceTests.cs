using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Foundations.Notes.Exceptions;
using Paperless.Mcp.Models.Processings.Notes;
using Paperless.Mcp.Models.Processings.Notes.Exceptions;
using Paperless.Mcp.Services.Foundations.Notes;
using Paperless.Mcp.Services.Processings.Notes;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Notes
{
    [TestClass]
    public class DocumentNoteProcessingServiceTests
    {
        private const int DocumentId = 42;

        private readonly Mock<IDocumentNoteService> documentNoteServiceMock;
        private readonly IDocumentNoteProcessingService documentNoteProcessingService;

        public DocumentNoteProcessingServiceTests()
        {
            this.documentNoteServiceMock = new Mock<IDocumentNoteService>();

            this.documentNoteProcessingService =
                new DocumentNoteProcessingService(
                    documentNoteService: this.documentNoteServiceMock.Object);
        }

        private static List<DocumentNote> CreateNotes() =>
        [
            new DocumentNote { Id = 7, Note = "Rechnung geprueft", Document = DocumentId },
            new DocumentNote { Id = 8, Note = "Zahlung angewiesen", Document = DocumentId },
        ];

        // Der wichtigste Test dieser Schicht: ohne Bestaetigung darf nichts verschwinden.
        [TestMethod]
        public async Task ShouldNotDeleteOnDryRunAsync()
        {
            // given
            List<DocumentNote> storedNotes = CreateNotes();

            this.documentNoteServiceMock.Setup(service =>
                service.RetrieveDocumentNotesAsync(DocumentId))
                    .ReturnsAsync(storedNotes);

            // when
            DocumentNoteRemovalPlan actualPlan =
                await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    DocumentId,
                    noteId: 7,
                    confirm: false);

            // then
            actualPlan.Removed.Should().BeFalse();
            actualPlan.DocumentId.Should().Be(DocumentId);
            actualPlan.NoteId.Should().Be(7);
            actualPlan.Note.Should().Be("Rechnung geprueft");
            actualPlan.NoteCount.Should().Be(2);
            actualPlan.Summary.Should().Contain("Dry run");
            actualPlan.Summary.Should().Contain("Rechnung geprueft");

            this.documentNoteServiceMock.Verify(service =>
                service.RetrieveDocumentNotesAsync(DocumentId),
                    Times.Once);

            this.documentNoteServiceMock.Verify(service =>
                service.RemoveDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.documentNoteServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDeleteWhenConfirmedAsync()
        {
            // given
            List<DocumentNote> storedNotes = CreateNotes();

            this.documentNoteServiceMock.Setup(service =>
                service.RetrieveDocumentNotesAsync(DocumentId))
                    .ReturnsAsync(storedNotes);

            this.documentNoteServiceMock.Setup(service =>
                service.RemoveDocumentNoteAsync(DocumentId, 7))
                    .ReturnsAsync([storedNotes[1]]);

            // when
            DocumentNoteRemovalPlan actualPlan =
                await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    DocumentId,
                    noteId: 7,
                    confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();
            actualPlan.NoteId.Should().Be(7);
            actualPlan.Summary.Should().NotContain("Dry run");
            actualPlan.Summary.Should().Contain("Rechnung geprueft");

            // Erst lesen, dann loeschen: der Bericht soll den Inhalt der Notiz auch im
            // bestaetigten Fall nennen koennen.
            this.documentNoteServiceMock.Verify(service =>
                service.RetrieveDocumentNotesAsync(DocumentId),
                    Times.Once);

            this.documentNoteServiceMock.Verify(service =>
                service.RemoveDocumentNoteAsync(DocumentId, 7),
                    Times.Once);

            this.documentNoteServiceMock.VerifyNoOtherCalls();
        }

        // Eine Id, die das Dokument nicht traegt, waere bei Paperless ein stiller
        // Fehlschlag. Hier wird sie zum Einwand.
        [TestMethod]
        public async Task ShouldThrowValidationExceptionIfNoteDoesNotBelongToDocumentAsync()
        {
            // given
            this.documentNoteServiceMock.Setup(service =>
                service.RetrieveDocumentNotesAsync(DocumentId))
                    .ReturnsAsync(CreateNotes());

            // when
            Func<Task<DocumentNoteRemovalPlan>> removeTask = async () =>
                await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    DocumentId,
                    noteId: 999,
                    confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentNoteProcessingValidationException>();

            this.documentNoteServiceMock.Verify(service =>
                service.RetrieveDocumentNotesAsync(DocumentId),
                    Times.Once);

            this.documentNoteServiceMock.Verify(service =>
                service.RemoveDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.documentNoteServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0, 7)]
        [DataRow(-1, 7)]
        [DataRow(42, 0)]
        [DataRow(42, -1)]
        public async Task ShouldThrowValidationExceptionIfIdsAreInvalidAsync(
            int invalidDocumentId,
            int invalidNoteId)
        {
            // when
            Func<Task<DocumentNoteRemovalPlan>> removeTask = async () =>
                await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    invalidDocumentId,
                    invalidNoteId,
                    confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentNoteProcessingValidationException>();

            // Eine ungueltige Id darf gar nicht erst beim Dienst ankommen
            this.documentNoteServiceMock.Verify(service =>
                service.RetrieveDocumentNotesAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentNoteServiceMock.Verify(service =>
                service.RemoveDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.documentNoteServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapNotFoundToDependencyValidationAsync()
        {
            // given
            var documentNoteDependencyValidationException =
                new DocumentNoteDependencyValidationException(
                    message: "Document note dependency validation error occurred, "
                        + "fix the errors and try again.",
                    innerException: new NotFoundDocumentNoteException(
                        message: "Document or note not found.",
                        innerException: new Exception()));

            this.documentNoteServiceMock.Setup(service =>
                service.RetrieveDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(documentNoteDependencyValidationException);

            // when
            Func<Task<DocumentNoteRemovalPlan>> removeTask = async () =>
                await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    DocumentId,
                    noteId: 7,
                    confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentNoteProcessingDependencyValidationException>();

            this.documentNoteServiceMock.Verify(service =>
                service.RetrieveDocumentNotesAsync(It.IsAny<int>()),
                    Times.Once);

            // Gescheitertes Lesen heisst: nicht loeschen
            this.documentNoteServiceMock.Verify(service =>
                service.RemoveDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.documentNoteServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapUnexpectedErrorToProcessingServiceExceptionAsync()
        {
            // given
            this.documentNoteServiceMock.Setup(service =>
                service.RetrieveDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

            // when
            Func<Task<DocumentNoteRemovalPlan>> removeTask = async () =>
                await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    DocumentId,
                    noteId: 7,
                    confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<DocumentNoteProcessingServiceException>();

            this.documentNoteServiceMock.Verify(service =>
                service.RetrieveDocumentNotesAsync(It.IsAny<int>()),
                    Times.Once);

            this.documentNoteServiceMock.VerifyNoOtherCalls();
        }

        // Ein langer Notiztext gehoert nicht ungekuerzt in eine Rueckfrage.
        [TestMethod]
        public async Task ShouldShortenLongNoteTextInPlanAsync()
        {
            // given
            string longNote = new('a', 500);

            this.documentNoteServiceMock.Setup(service =>
                service.RetrieveDocumentNotesAsync(DocumentId))
                    .ReturnsAsync([
                        new DocumentNote { Id = 7, Note = longNote, Document = DocumentId }]);

            // when
            DocumentNoteRemovalPlan actualPlan =
                await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    DocumentId,
                    noteId: 7,
                    confirm: false);

            // then
            actualPlan.Note.Should().HaveLength(123);
            actualPlan.Note.Should().EndWith("...");

            this.documentNoteServiceMock.Verify(service =>
                service.RetrieveDocumentNotesAsync(DocumentId),
                    Times.Once);

            this.documentNoteServiceMock.VerifyNoOtherCalls();
        }
    }
}
