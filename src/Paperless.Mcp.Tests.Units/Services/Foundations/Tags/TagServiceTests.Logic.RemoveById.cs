using Moq;
using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    public partial class TagServiceTests
    {
        [TestMethod]
        public async Task ShouldRemoveTagByIdAsync()
        {
            // given
            Tag randomTag = CreateRandomTag();
            int inputTagId = randomTag.Id;

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteTagByIdAsync(inputTagId))
                    .Returns(ValueTask.CompletedTask);

            // when
            await this.tagService.RemoveTagByIdAsync(inputTagId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteTagByIdAsync(inputTagId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
