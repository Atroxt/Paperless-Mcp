using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Notes
{
    public partial class DocumentNoteServiceTests
    {
        [TestMethod]
        public async Task ShouldRemoveDocumentNoteAsync()
        {
            // given
            int inputDocumentId = GetRandomNumber();
            int inputNoteId = GetRandomNumber();
            List<DocumentNote> storedDocumentNotes = CreateRandomDocumentNotes(inputDocumentId);
            List<DocumentNote> expectedDocumentNotes = storedDocumentNotes.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteDocumentNoteAsync(inputDocumentId, inputNoteId))
                    .ReturnsAsync(storedDocumentNotes);

            // when
            List<DocumentNote> actualDocumentNotes =
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    inputDocumentId,
                    inputNoteId);

            // then
            actualDocumentNotes.Should().BeEquivalentTo(expectedDocumentNotes);

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(inputDocumentId, inputNoteId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
