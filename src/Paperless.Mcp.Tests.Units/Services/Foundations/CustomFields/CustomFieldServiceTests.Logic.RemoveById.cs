using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.CustomFields
{
    public partial class CustomFieldServiceTests
    {
        [TestMethod]
        public async Task ShouldRemoveCustomFieldByIdAsync()
        {
            // given
            CustomField randomCustomField = CreateRandomCustomField();
            int inputCustomFieldId = randomCustomField.Id;

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteCustomFieldByIdAsync(inputCustomFieldId))
                    .Returns(ValueTask.CompletedTask);

            // when
            await this.customFieldService.RemoveCustomFieldByIdAsync(inputCustomFieldId);

            // then
            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteCustomFieldByIdAsync(inputCustomFieldId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
