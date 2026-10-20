using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    public partial class TagServiceTests
    {
        [TestMethod]
        public async Task ShouldModifyTagAsync()
        {
            // given
            Tag randomTag = CreateRandomTag();
            Tag inputTag = randomTag;
            Tag modifiedTag = inputTag.DeepClone();
            Tag expectedTag = modifiedTag.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutTagAsync(inputTag))
                    .ReturnsAsync(modifiedTag);

            // when
            Tag actualTag = await this.tagService.ModifyTagAsync(inputTag);

            // then
            actualTag.Should().BeEquivalentTo(expectedTag);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutTagAsync(inputTag),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
