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
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageIsInvalidAndLogItAsync(
            int invalidPage)
        {
            // given
            int validPageSize = GetRandomNumber();

            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.");

            invalidDocumentTypeException.UpsertDataList(
                key: "Page",
                value: "Value must be greater than zero");

            var expectedDocumentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentTypeException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(invalidPage, validPageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidNumbers))]
        public async Task ShouldThrowValidationExceptionOnRetrievePageIfPageSizeIsInvalidAndLogItAsync(
            int invalidPageSize)
        {
            // given
            int validPage = GetRandomNumber();

            var invalidDocumentTypeException =
                new InvalidDocumentTypeException(
                    message: "DocumentType is invalid, fix the errors and try again.");

            invalidDocumentTypeException.UpsertDataList(
                key: "PageSize",
                value: "Value must be greater than zero");

            var expectedDocumentTypeValidationException =
                new DocumentTypeValidationException(
                    message: "DocumentType validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentTypeException);

            // when
            Func<Task<DocumentTypePage>> retrieveDocumentTypePageTask = async () =>
                await this.documentTypeService.RetrieveDocumentTypePageAsync(validPage, invalidPageSize);

            // then
            await retrieveDocumentTypePageTask.Should()
                .ThrowAsync<DocumentTypeValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentTypeValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentTypeValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentTypePageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
