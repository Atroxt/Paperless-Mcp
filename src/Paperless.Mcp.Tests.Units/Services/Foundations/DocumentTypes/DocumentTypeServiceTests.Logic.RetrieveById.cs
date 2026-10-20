using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveDocumentTypeByIdAsync()
        {
            // given
            DocumentType randomDocumentType = CreateRandomDocumentType();
            int inputDocumentTypeId = randomDocumentType.Id;
            DocumentType storedDocumentType = randomDocumentType;
            DocumentType expectedDocumentType = storedDocumentType.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypeByIdAsync(inputDocumentTypeId))
                    .ReturnsAsync(storedDocumentType);

            // when
            DocumentType actualDocumentType = await this.documentTypeService.RetrieveDocumentTypeByIdAsync(inputDocumentTypeId);

            // then
            actualDocumentType.Should().BeEquivalentTo(expectedDocumentType);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypeByIdAsync(inputDocumentTypeId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
