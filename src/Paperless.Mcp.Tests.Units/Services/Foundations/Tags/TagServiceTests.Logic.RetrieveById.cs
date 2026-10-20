using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    public partial class TagServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveTagByIdAsync()
        {
            // given
            Tag randomTag = CreateRandomTag();
            int inputTagId = randomTag.Id;
            Tag storedTag = randomTag;
            Tag expectedTag = storedTag.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagByIdAsync(inputTagId))
                    .ReturnsAsync(storedTag);

            // when
            Tag actualTag = await this.tagService.RetrieveTagByIdAsync(inputTagId);

            // then
            actualTag.Should().BeEquivalentTo(expectedTag);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(inputTagId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
