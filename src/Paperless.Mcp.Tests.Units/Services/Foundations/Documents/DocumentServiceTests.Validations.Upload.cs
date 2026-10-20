using System.Text;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnUploadIfUploadIsNullAndLogItAsync()
        {
            // given
            DocumentUpload? nullUpload = null;

            var nullDocumentException =
                new NullDocumentException(message: "DocumentUpload is null.");

            var expectedValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: nullDocumentException);

            // when
            Func<Task<string>> uploadTask = async () =>
                await this.documentService.AddDocumentFileAsync(nullUpload!);

            // then
            await uploadTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentFileAsync(It.IsAny<PaperlessDocumentForm>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public async Task ShouldThrowValidationExceptionOnUploadIfFileNameIsInvalidAndLogItAsync(
            string? invalidFileName)
        {
            // given
            var invalidUpload = new DocumentUpload
            {
                FileName = invalidFileName!,
                Content = Encoding.UTF8.GetBytes("inhalt"),
            };

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentUpload.FileName),
                value: "Text is required");

            var expectedValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<string>> uploadTask = async () =>
                await this.documentService.AddDocumentFileAsync(invalidUpload);

            // then
            await uploadTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentFileAsync(It.IsAny<PaperlessDocumentForm>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionOnUploadIfContentIsEmptyAndLogItAsync()
        {
            // given
            var invalidUpload = new DocumentUpload
            {
                FileName = "leer.txt",
                Content = [],
            };

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            invalidDocumentException.UpsertDataList(
                key: nameof(DocumentUpload.Content),
                value: "File content is required");

            var expectedValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            // when
            Func<Task<string>> uploadTask = async () =>
                await this.documentService.AddDocumentFileAsync(invalidUpload);

            // then
            await uploadTask.Should()
                .ThrowAsync<DocumentValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostDocumentFileAsync(It.IsAny<PaperlessDocumentForm>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }
    }
}
