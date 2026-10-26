using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Notes
{
    public partial class DocumentNoteServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveDocumentNotesAsync()
        {
            // given
            int inputDocumentId = GetRandomNumber();
            List<DocumentNote> storedDocumentNotes = CreateRandomDocumentNotes(inputDocumentId);
            List<DocumentNote> expectedDocumentNotes = storedDocumentNotes.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentNotesAsync(inputDocumentId))
                    .ReturnsAsync(storedDocumentNotes);

            // when
            List<DocumentNote> actualDocumentNotes =
                await this.documentNoteService.RetrieveDocumentNotesAsync(inputDocumentId);

            // then
            actualDocumentNotes.Should().BeEquivalentTo(expectedDocumentNotes);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(inputDocumentId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Paperless gibt das Dokument nicht mit zurueck -- die Service traegt es nach,
        // damit eine einzelne Notiz fuer sich selbst sprechen kann.
        [TestMethod]
        public async Task ShouldAssignDocumentIdOnRetrieveDocumentNotesAsync()
        {
            // given
            int inputDocumentId = GetRandomNumber();
            List<DocumentNote> storedDocumentNotes = CreateRandomDocumentNotes(documentId: 0);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentNotesAsync(inputDocumentId))
                    .ReturnsAsync(storedDocumentNotes);

            // when
            List<DocumentNote> actualDocumentNotes =
                await this.documentNoteService.RetrieveDocumentNotesAsync(inputDocumentId);

            // then
            actualDocumentNotes.Should().OnlyContain(documentNote =>
                documentNote.Document == inputDocumentId);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(inputDocumentId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
