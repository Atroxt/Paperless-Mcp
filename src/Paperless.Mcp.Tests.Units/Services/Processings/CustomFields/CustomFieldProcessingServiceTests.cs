using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;
using Paperless.Mcp.Models.Processings.CustomFields;
using Paperless.Mcp.Models.Processings.CustomFields.Exceptions;
using Paperless.Mcp.Services.Foundations.CustomFields;
using Paperless.Mcp.Services.Processings.CustomFields;

namespace Paperless.Mcp.Tests.Units.Services.Processings.CustomFields
{
    [TestClass]
    public class CustomFieldProcessingServiceTests
    {
        private readonly Mock<ICustomFieldService> customFieldServiceMock;
        private readonly ICustomFieldProcessingService customFieldProcessingService;

        public CustomFieldProcessingServiceTests()
        {
            this.customFieldServiceMock = new Mock<ICustomFieldService>();

            this.customFieldProcessingService =
                new CustomFieldProcessingService(customFieldService: this.customFieldServiceMock.Object);
        }

        private static CustomField CreateCustomField(int documentCount) =>
            new() { Id = 7, Name = "Rechnungen", DocumentCount = documentCount };

        // Der wichtigste Test dieser Schicht: ohne Bestaetigung darf nichts verschwinden.
        [TestMethod]
        public async Task ShouldNotDeleteOnDryRunAsync()
        {
            // given
            CustomField storedCustomField = CreateCustomField(documentCount: 143);

            this.customFieldServiceMock.Setup(service =>
                service.RetrieveCustomFieldByIdAsync(storedCustomField.Id))
                    .ReturnsAsync(storedCustomField);

            // when
            CustomFieldRemovalPlan actualPlan =
                await this.customFieldProcessingService.RemoveCustomFieldByIdAsync(storedCustomField.Id, confirm: false);

            // then
            actualPlan.Removed.Should().BeFalse();
            actualPlan.CustomFieldId.Should().Be(storedCustomField.Id);
            actualPlan.Name.Should().Be("Rechnungen");
            actualPlan.DocumentCount.Should().Be(143);
            actualPlan.Summary.Should().Contain("Dry run");
            actualPlan.Summary.Should().Contain("143");

            this.customFieldServiceMock.Verify(service =>
                service.RetrieveCustomFieldByIdAsync(storedCustomField.Id),
                    Times.Once);

            this.customFieldServiceMock.Verify(service =>
                service.RemoveCustomFieldByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.customFieldServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldDeleteWhenConfirmedAsync()
        {
            // given
            CustomField storedCustomField = CreateCustomField(documentCount: 2);

            this.customFieldServiceMock.Setup(service =>
                service.RetrieveCustomFieldByIdAsync(storedCustomField.Id))
                    .ReturnsAsync(storedCustomField);

            // when
            CustomFieldRemovalPlan actualPlan =
                await this.customFieldProcessingService.RemoveCustomFieldByIdAsync(storedCustomField.Id, confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();
            actualPlan.DocumentCount.Should().Be(2);
            actualPlan.Summary.Should().NotContain("Dry run");

            // Erst lesen, dann loeschen: der Bericht soll die Tragweite auch im
            // bestaetigten Fall nennen koennen.
            this.customFieldServiceMock.Verify(service =>
                service.RetrieveCustomFieldByIdAsync(storedCustomField.Id),
                    Times.Once);

            this.customFieldServiceMock.Verify(service =>
                service.RemoveCustomFieldByIdAsync(storedCustomField.Id),
                    Times.Once);

            this.customFieldServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportWhenNoDocumentCarriesTheCustomFieldAsync()
        {
            // given
            CustomField storedCustomField = CreateCustomField(documentCount: 0);

            this.customFieldServiceMock.Setup(service =>
                service.RetrieveCustomFieldByIdAsync(storedCustomField.Id))
                    .ReturnsAsync(storedCustomField);

            // when
            CustomFieldRemovalPlan actualPlan =
                await this.customFieldProcessingService.RemoveCustomFieldByIdAsync(storedCustomField.Id, confirm: false);

            // then
            actualPlan.Summary.Should().Contain("No document");

            this.customFieldServiceMock.Verify(service =>
                service.RetrieveCustomFieldByIdAsync(storedCustomField.Id),
                    Times.Once);

            this.customFieldServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task ShouldThrowValidationExceptionIfIdIsInvalidAsync(int invalidCustomFieldId)
        {
            // when
            Func<Task<CustomFieldRemovalPlan>> removeTask = async () =>
                await this.customFieldProcessingService.RemoveCustomFieldByIdAsync(invalidCustomFieldId, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<CustomFieldProcessingValidationException>();

            // Eine ungueltige Id darf gar nicht erst beim Dienst ankommen
            this.customFieldServiceMock.Verify(service =>
                service.RetrieveCustomFieldByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.customFieldServiceMock.Verify(service =>
                service.RemoveCustomFieldByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.customFieldServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapNotFoundToDependencyValidationAsync()
        {
            // given
            var customFieldDependencyValidationException =
                new CustomFieldDependencyValidationException(
                    message: "CustomField dependency validation error occurred, fix the errors and try again.",
                    innerException: new NotFoundCustomFieldException(
                        message: "CustomField not found.",
                        innerException: new Exception()));

            this.customFieldServiceMock.Setup(service =>
                service.RetrieveCustomFieldByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(customFieldDependencyValidationException);

            // when
            Func<Task<CustomFieldRemovalPlan>> removeTask = async () =>
                await this.customFieldProcessingService.RemoveCustomFieldByIdAsync(99, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<CustomFieldProcessingDependencyValidationException>();

            this.customFieldServiceMock.Verify(service =>
                service.RetrieveCustomFieldByIdAsync(It.IsAny<int>()),
                    Times.Once);

            // Gescheitertes Lesen heisst: nicht loeschen
            this.customFieldServiceMock.Verify(service =>
                service.RemoveCustomFieldByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.customFieldServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapUnexpectedErrorToProcessingServiceExceptionAsync()
        {
            // given
            this.customFieldServiceMock.Setup(service =>
                service.RetrieveCustomFieldByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(new Exception());

            // when
            Func<Task<CustomFieldRemovalPlan>> removeTask = async () =>
                await this.customFieldProcessingService.RemoveCustomFieldByIdAsync(7, confirm: true);

            // then
            await removeTask.Should()
                .ThrowAsync<CustomFieldProcessingServiceException>();

            this.customFieldServiceMock.Verify(service =>
                service.RetrieveCustomFieldByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.customFieldServiceMock.VerifyNoOtherCalls();
        }
    }
}
