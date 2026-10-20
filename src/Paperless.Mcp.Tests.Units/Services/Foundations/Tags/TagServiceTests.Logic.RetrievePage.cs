using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    public partial class TagServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveTagPageAsync()
        {
            // given
            int inputPage = GetRandomNumber();
            int inputPageSize = GetRandomNumber();
            TagPage randomTagPage = CreateRandomTagPage();
            TagPage storedTagPage = randomTagPage;
            TagPage expectedTagPage = storedTagPage.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(inputPage, inputPageSize))
                    .ReturnsAsync(storedTagPage);

            // when
            TagPage actualTagPage =
                await this.tagService.RetrieveTagPageAsync(inputPage, inputPageSize);

            // then
            actualTagPage.Should().BeEquivalentTo(expectedTagPage);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(inputPage, inputPageSize),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
