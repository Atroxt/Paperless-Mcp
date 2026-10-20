using AwesomeAssertions;
using Force.DeepCloner;
using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    public partial class CustomFieldServiceTests
    {
        [TestMethod]
        public async Task ShouldRetrieveCustomFieldByIdAsync()
        {
            // given
            CustomField randomCustomField = CreateRandomCustomField();
            int inputCustomFieldId = randomCustomField.Id;
            CustomField storedCustomField = randomCustomField;
            CustomField expectedCustomField = storedCustomField.DeepClone();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetCustomFieldByIdAsync(inputCustomFieldId))
                    .ReturnsAsync(storedCustomField);

            // when
            CustomField actualCustomField = await this.customFieldService.RetrieveCustomFieldByIdAsync(inputCustomFieldId);

            // then
            actualCustomField.Should().BeEquivalentTo(expectedCustomField);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetCustomFieldByIdAsync(inputCustomFieldId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
