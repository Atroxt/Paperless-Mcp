using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeServiceTests
    {
        [TestMethod]
        public async Task ShouldRemoveDocumentTypeByIdAsync()
        {
            // given
            DocumentType randomDocumentType = CreateRandomDocumentType();
            int inputDocumentTypeId = randomDocumentType.Id;

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteDocumentTypeByIdAsync(inputDocumentTypeId))
                    .Returns(ValueTask.CompletedTask);

            // when
            await this.documentTypeService.RemoveDocumentTypeByIdAsync(inputDocumentTypeId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentTypeByIdAsync(inputDocumentTypeId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
