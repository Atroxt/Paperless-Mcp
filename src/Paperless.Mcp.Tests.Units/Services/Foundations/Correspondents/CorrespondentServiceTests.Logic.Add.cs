using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldAddCorrespondentAsync()
        {
            // given
            Correspondent randomCorrespondent = CreateRandomCorrespondent();
            Correspondent inputCorrespondent = randomCorrespondent;
            Correspondent addedCorrespondent = inputCorrespondent.DeepClone();
            Correspondent expectedCorrespondent = addedCorrespondent.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostCorrespondentAsync(inputCorrespondent))
                    .ReturnsAsync(addedCorrespondent);

            // when
            Correspondent actualCorrespondent = await this.correspondentService.AddCorrespondentAsync(inputCorrespondent);

            // then
            actualCorrespondent.Should().BeEquivalentTo(expectedCorrespondent);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCorrespondentAsync(inputCorrespondent),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
