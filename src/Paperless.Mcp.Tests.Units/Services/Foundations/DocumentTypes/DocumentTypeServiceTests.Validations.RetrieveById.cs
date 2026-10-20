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
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRetrieveByIdIfIdIsInvalidAndLogItAsync(
            int invalidDocumentTypeId)
        {
            // given
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
            Func<Task<DocumentType>> retrieveDocumentTypeByIdTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypeByIdAsync(invalidDocumentTypeId);

            // then
            await retrieveDocumentTypeByIdTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypeByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
