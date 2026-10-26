using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Notes
{
    public partial class DocumentNoteServiceTests
    {
        [TestMethod]
        public async Task ShouldAddDocumentNoteAsync()
        {
            // given
            int inputDocumentId = GetRandomNumber();
            string inputNote = GetRandomText();
            List<DocumentNote> storedDocumentNotes = CreateRandomDocumentNotes(inputDocumentId);
            List<DocumentNote> expectedDocumentNotes = storedDocumentNotes.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentNoteAsync(
                    inputDocumentId,
                    It.Is<PaperlessNote>(note => note.Note == inputNote)))
                        .ReturnsAsync(storedDocumentNotes);

            // when
            List<DocumentNote> actualDocumentNotes =
                await this.documentNoteService.AddDocumentNoteAsync(inputDocumentId, inputNote);

            // then
            actualDocumentNotes.Should().BeEquivalentTo(expectedDocumentNotes);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentNoteAsync(
                    inputDocumentId,
                    It.Is<PaperlessNote>(note => note.Note == inputNote)),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
