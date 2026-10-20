using Moq;
using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Correspondents
{
    public partial class CorrespondentServiceTests
    {
        [TestMethod]
        public async Task ShouldRemoveCorrespondentByIdAsync()
        {
            // given
            Correspondent randomCorrespondent = CreateRandomCorrespondent();
            int inputCorrespondentId = randomCorrespondent.Id;

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCorrespondentByIdAsync(inputCorrespondentId))
                    .Returns(ValueTask.CompletedTask);

            // when
            await this.correspondentService.RemoveCorrespondentByIdAsync(inputCorrespondentId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCorrespondentByIdAsync(inputCorrespondentId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
