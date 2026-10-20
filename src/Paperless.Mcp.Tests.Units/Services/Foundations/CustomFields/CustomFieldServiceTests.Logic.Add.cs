using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    public partial class CustomFieldServiceTests
    {
        [TestMethod]
        public async Task ShouldAddCustomFieldAsync()
        {
            // given
            CustomField randomCustomField = CreateRandomCustomField();
            CustomField inputCustomField = randomCustomField;
            CustomField addedCustomField = inputCustomField.DeepClone();
            CustomField expectedCustomField = addedCustomField.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostCustomFieldAsync(inputCustomField))
                    .ReturnsAsync(addedCustomField);

            // when
            CustomField actualCustomField = await this.customFieldService.AddCustomFieldAsync(inputCustomField);

            // then
            actualCustomField.Should().BeEquivalentTo(expectedCustomField);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostCustomFieldAsync(inputCustomField),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
