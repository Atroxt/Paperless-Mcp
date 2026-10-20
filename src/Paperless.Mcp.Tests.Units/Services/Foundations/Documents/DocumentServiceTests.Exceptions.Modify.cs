using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Documents
{
    public partial class DocumentServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            var someUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                Title = "egal",
            };
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedDocumentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: invalidDocumentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<Document>> modifyDocumentTask = async () =>
                await this.documentService.ModifyDocumentAsync(someUpdate);

            // then
            await modifyDocumentTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfDocumentIsNotFoundAndLogItAsync()
        {
            // given
            var someUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                Title = "egal",
            };
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundDocumentException =
                new NotFoundDocumentException(
                    message: "Document not found.",
                    innerException: httpResponseNotFoundException);

            var expectedDocumentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundDocumentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<Document>> modifyDocumentTask = async () =>
                await this.documentService.ModifyDocumentAsync(someUpdate);

            // then
            await modifyDocumentTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(CriticalDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnModifyIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            var someUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                Title = "egal",
            };

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<Document>> modifyDocumentTask = async () =>
                await this.documentService.ModifyDocumentAsync(someUpdate);

            // then
            await modifyDocumentTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnModifyIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            var someUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                Title = "egal",
            };

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<Document>> modifyDocumentTask = async () =>
                await this.documentService.ModifyDocumentAsync(someUpdate);

            // then
            await modifyDocumentTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnModifyIfTransportErrorOccursAndLogItAsync()
        {
            // given
            var someUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                Title = "egal",
            };
            var httpRequestException = new HttpRequestException();

            var failedDocumentDependencyException =
                new FailedDocumentDependencyException(
                    message: "Failed document dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedDocumentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: failedDocumentDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<Document>> modifyDocumentTask = async () =>
                await this.documentService.ModifyDocumentAsync(someUpdate);

            // then
            await modifyDocumentTask.Should()
                .ThrowAsync<DocumentDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnModifyIfServiceErrorOccursAndLogItAsync()
        {
            // given
            var someUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                Title = "egal",
            };
            var serviceException = new Exception();

            var failedDocumentServiceException =
                new FailedDocumentServiceException(
                    message: "Failed document service error occurred, contact support.",
                    innerException: serviceException);

            var expectedDocumentServiceException =
                new DocumentServiceException(
                    message: "Document service error occurred, contact support.",
                    innerException: failedDocumentServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<Document>> modifyDocumentTask = async () =>
                await this.documentService.ModifyDocumentAsync(someUpdate);

            // then
            await modifyDocumentTask.Should()
                .ThrowAsync<DocumentServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnModifyIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            var someUpdate = new DocumentUpdate
            {
                DocumentId = GetRandomNumber(),
                Title = "egal",
            };

            var unauthorizedDocumentException =
                new UnauthorizedDocumentException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedDocumentDependencyValidationException =
                new DocumentDependencyValidationException(
                    message: "Document dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedDocumentException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<Document>> retrieveDocumentByIdAsyncTask = async () =>
                await this.documentService.ModifyDocumentAsync(someUpdate);

            // then
            await retrieveDocumentByIdAsyncTask.Should()
                .ThrowAsync<DocumentDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDocumentDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PatchDocumentAsync(It.IsAny<int>(), It.IsAny<PaperlessDocumentUpdate>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDocumentDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

    }
}
