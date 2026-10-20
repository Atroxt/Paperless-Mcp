using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    public partial class TagServiceTests
    {
        [TestMethod]
        public async Task ShouldAddTagAsync()
        {
            // given
            Tag randomTag = CreateRandomTag();
            Tag inputTag = randomTag;
            Tag addedTag = inputTag.DeepClone();
            Tag expectedTag = addedTag.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostTagAsync(inputTag))
                    .ReturnsAsync(addedTag);

            // when
            Tag actualTag = await this.tagService.AddTagAsync(inputTag);

            // then
            actualTag.Should().BeEquivalentTo(expectedTag);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(inputTag),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
