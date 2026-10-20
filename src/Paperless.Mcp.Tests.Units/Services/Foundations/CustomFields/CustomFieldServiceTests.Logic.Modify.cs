using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    public partial class CustomFieldServiceTests
    {
        [TestMethod]
        public async Task ShouldModifyCustomFieldAsync()
        {
            // given
            CustomField randomCustomField = CreateRandomCustomField();
            CustomField inputCustomField = randomCustomField;
            CustomField modifiedCustomField = inputCustomField.DeepClone();
            CustomField expectedCustomField = modifiedCustomField.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.PutCustomFieldAsync(inputCustomField))
                    .ReturnsAsync(modifiedCustomField);

            // when
            CustomField actualCustomField = await this.customFieldService.ModifyCustomFieldAsync(inputCustomField);

            // then
            actualCustomField.Should().BeEquivalentTo(expectedCustomField);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutCustomFieldAsync(inputCustomField),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
