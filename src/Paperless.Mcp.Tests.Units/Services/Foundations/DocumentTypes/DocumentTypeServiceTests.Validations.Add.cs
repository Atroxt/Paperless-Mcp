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
        public async Task ShouldThrowValidationExceptionOnAddIfDocumentTypeIsNullAndLogItAsync()
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
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(nullDocumentType!);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionOnAddIfDocumentTypeNameIsInvalidAndLogItAsync(
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
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(invalidDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfDocumentTypeNameExceedsMaximumLengthAndLogItAsync()
        {
            // given
            DocumentType invalidDocumentType = CreateRandomDocumentType();
            invalidDocumentType.Name = new string('x', count: 129);

            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.");

            invalidDocumentTypeException.UpsertDataList(
                key: nameof(DocumentType.Name),
                value: "Text exceeds the maximum length of 128 characters");

            var expectedDocumentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentTypeException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(invalidDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnAddIfMatchingAlgorithmIsUndefinedAndLogItAsync()
        {
            // given
            DocumentType invalidDocumentType = CreateRandomDocumentType();
            invalidDocumentType.MatchingAlgorithm = (DocumentTypeMatchingAlgorithm)99;

            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.");

            invalidDocumentTypeException.UpsertDataList(
                key: nameof(DocumentType.MatchingAlgorithm),
                value: "Value is not a defined matching algorithm");

            var expectedDocumentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentTypeException);

            // when
            Func<Task<DocumentType>> addDocumentTypeTask = async () =>
                await this.documentTypeService.AddDocumentTypeAsync(invalidDocumentType);

            // then
            await addDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentTypeAsync(It.IsAny<DocumentType>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
