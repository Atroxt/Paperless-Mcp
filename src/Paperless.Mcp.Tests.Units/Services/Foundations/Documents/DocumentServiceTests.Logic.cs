using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveDocumentByIdAsync()
        {
            // given
            Document randomDocument = CreateRandomDocument();
            int inputDocumentId = randomDocument.Id;
            Document storedDocument = randomDocument;
            Document expectedDocument = storedDocument.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentByIdAsync(inputDocumentId))
                    .ReturnsAsync(storedDocument);

            // when
            Document actualDocument =
                await this.documentService.RetrieveDocumentByIdAsync(inputDocumentId);

            // then
            actualDocument.Should().BeEquivalentTo(expectedDocument);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentByIdAsync(inputDocumentId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldRetrieveDocumentPageAsync()
        {
            // given
            DocumentSearch inputSearch = CreateRandomDocumentSearch();
            DocumentPage randomPage = CreateRandomDocumentPage();
            DocumentPage storedPage = randomPage;
            DocumentPage expectedPage = storedPage.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentPageAsync(inputSearch))
                    .ReturnsAsync(storedPage);

            // when
            DocumentPage actualPage =
                await this.documentService.RetrieveDocumentPageAsync(inputSearch);

            // then
            actualPage.Should().BeEquivalentTo(expectedPage);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentPageAsync(inputSearch),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
