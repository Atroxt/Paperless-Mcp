using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    public partial class CustomFieldServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveCustomFieldPageAsync()
        {
            // given
            int inputPage = GetRandomNumber();
            int inputPageSize = GetRandomNumber();
            CustomFieldPage randomCustomFieldPage = CreateRandomCustomFieldPage();
            CustomFieldPage storedCustomFieldPage = randomCustomFieldPage;
            CustomFieldPage expectedCustomFieldPage = storedCustomFieldPage.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldPageAsync(inputPage, inputPageSize))
                    .ReturnsAsync(storedCustomFieldPage);

            // when
            CustomFieldPage actualCustomFieldPage =
                await this.customFieldService.RetrieveCustomFieldPageAsync(inputPage, inputPageSize);

            // then
            actualCustomFieldPage.Should().BeEquivalentTo(expectedCustomFieldPage);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldPageAsync(inputPage, inputPageSize),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
