using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldModifyCorrespondentAsync()
        {
            // given
            Correspondent randomCorrespondent = CreateRandomCorrespondent();
            Correspondent inputCorrespondent = randomCorrespondent;
            Correspondent modifiedCorrespondent = inputCorrespondent.DeepClone();
            Correspondent expectedCorrespondent = modifiedCorrespondent.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutCorrespondentAsync(inputCorrespondent))
                    .ReturnsAsync(modifiedCorrespondent);

            // when
            Correspondent actualCorrespondent = await this.correspondentService.ModifyCorrespondentAsync(inputCorrespondent);

            // then
            actualCorrespondent.Should().BeEquivalentTo(expectedCorrespondent);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCorrespondentAsync(inputCorrespondent),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
