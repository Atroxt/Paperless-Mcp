using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnModifyIfDocumentTypeIsNullAndLogItAsync()
        {
            // given
            DocumentType? nullDocumentType = null;

            var nullDocumentTypeException =
                new NullDocumentTypeException(message: "DocumentType is null.");

            var expectedDocumentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: nullDocumentTypeException);

            // when
            Func<Task<DocumentType>> modifyDocumentTypeTask = async () =>
                await this.documentTypeService.ModifyDocumentTypeAsync(nullDocumentType!);

            // then
            await modifyDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnModifyIfIdIsInvalidAndLogItAsync(
            int invalidId)
        {
            // given
            DocumentType invalidDocumentType = CreateRandomDocumentType();
            invalidDocumentType.Id = invalidId;

            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.");

            invalidDocumentTypeException.UpsertDataList(
                key: nameof(DocumentType.Id),
                value: "Value must be greater than zero");

            var expectedDocumentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentTypeException);

            // when
            Func<Task<DocumentType>> modifyDocumentTypeTask = async () =>
                await this.documentTypeService.ModifyDocumentTypeAsync(invalidDocumentType);

            // then
            await modifyDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnModifyIfDocumentTypeNameIsInvalidAndLogItAsync(
            string? invalidName)
        {
            // given
            DocumentType invalidDocumentType = CreateRandomDocumentType();
            invalidDocumentType.Name = invalidName!;

            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.");

            invalidDocumentTypeException.UpsertDataList(
                key: nameof(DocumentType.Name),
                value: "Text is required");

            var expectedDocumentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentTypeException);

            // when
            Func<Task<DocumentType>> modifyDocumentTypeTask = async () =>
                await this.documentTypeService.ModifyDocumentTypeAsync(invalidDocumentType);

            // then
            await modifyDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PutDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
