using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeServiceTests
    {
        [TestMethod]
        public async Task ShouldModifyDocumentTypeAsync()
        {
            // given
            DocumentType randomDocumentType = CreateRandomDocumentType();
            DocumentType inputDocumentType = randomDocumentType;
            DocumentType modifiedDocumentType = inputDocumentType.DeepClone();
            DocumentType expectedDocumentType = modifiedDocumentType.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutDocumentTypeAsync(inputDocumentType))
                    .ReturnsAsync(modifiedDocumentType);

            // when
            DocumentType actualDocumentType = await this.documentTypeService.ModifyDocumentTypeAsync(inputDocumentType);

            // then
            actualDocumentType.Should().BeEquivalentTo(expectedDocumentType);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutDocumentTypeAsync(inputDocumentType),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
