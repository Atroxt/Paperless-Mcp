using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveCorrespondentPageAsync()
        {
            // given
            int inputPage = GetRandomNumber();
            int inputPageSize = GetRandomNumber();
            CorrespondentPage randomCorrespondentPage = CreateRandomCorrespondentPage();
            CorrespondentPage storedCorrespondentPage = randomCorrespondentPage;
            CorrespondentPage expectedCorrespondentPage = storedCorrespondentPage.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCorrespondentPageAsync(inputPage, inputPageSize))
                    .ReturnsAsync(storedCorrespondentPage);

            // when
            CorrespondentPage actualCorrespondentPage =
                await this.correspondentService.RetrieveCorrespondentPageAsync(inputPage, inputPageSize);

            // then
            actualCorrespondentPage.Should().BeEquivalentTo(expectedCorrespondentPage);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCorrespondentPageAsync(inputPage, inputPageSize),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
