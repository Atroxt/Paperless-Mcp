using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveCorrespondentByIdAsync()
        {
            // given
            Correspondent randomCorrespondent = CreateRandomCorrespondent();
            int inputCorrespondentId = randomCorrespondent.Id;
            Correspondent storedCorrespondent = randomCorrespondent;
            Correspondent expectedCorrespondent = storedCorrespondent.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCorrespondentByIdAsync(inputCorrespondentId))
                    .ReturnsAsync(storedCorrespondent);

            // when
            Correspondent actualCorrespondent = await this.correspondentService.RetrieveCorrespondentByIdAsync(inputCorrespondentId);

            // then
            actualCorrespondent.Should().BeEquivalentTo(expectedCorrespondent);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCorrespondentByIdAsync(inputCorrespondentId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
