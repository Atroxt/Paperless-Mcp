using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveDocumentTypePageAsync()
        {
            // given
            int inputPage = GetRandomNumber();
            int inputPageSize = GetRandomNumber();
            DocumentTypePage randomDocumentTypePage = CreateRandomDocumentTypePage();
            DocumentTypePage storedDocumentTypePage = randomDocumentTypePage;
            DocumentTypePage expectedDocumentTypePage = storedDocumentTypePage.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentTypePageAsync(inputPage, inputPageSize))
                    .ReturnsAsync(storedDocumentTypePage);

            // when
            DocumentTypePage actualDocumentTypePage =
                await this.documentTypeService.RetrieveDocumentTypePageAsync(inputPage, inputPageSize);

            // then
            actualDocumentTypePage.Should().BeEquivalentTo(expectedDocumentTypePage);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(inputPage, inputPageSize),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
