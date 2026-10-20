using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeServiceTests
    {
        [TestMethod]
        public async Task ShouldAddDocumentTypeAsync()
        {
            // given
            DocumentType randomDocumentType = CreateRandomDocumentType();
            DocumentType inputDocumentType = randomDocumentType;
            DocumentType addedDocumentType = inputDocumentType.DeepClone();
            DocumentType expectedDocumentType = addedDocumentType.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostDocumentTypeAsync(inputDocumentType))
                    .ReturnsAsync(addedDocumentType);

            // when
            DocumentType actualDocumentType = await this.documentTypeService.AddDocumentTypeAsync(inputDocumentType);

            // then
            actualDocumentType.Should().BeEquivalentTo(expectedDocumentType);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(inputDocumentType),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
