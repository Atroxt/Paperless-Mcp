using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Foundations.Notes.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Notes
{
    public partial class DocumentNoteServiceTests
    {
        // Die haeufigste Verwechslung beim Loeschen: eine Notiz-Id, die es nicht gibt.
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveIfNoteIsNotFoundAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
            int someNoteId = GetRandomNumber();
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundDocumentNoteException =
                new NotFoundDocumentNoteException(
                    message: "Document or note not found.",
                    innerException: httpResponseNotFoundException);

            var expectedDependencyValidationException =
                new DocumentNoteDependencyValidationException(
                    message: "Document note dependency validation error occurred, "
                        + "fix the errors and try again.",
                    innerException: notFoundDocumentNoteException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<List<DocumentNote>>> removeDocumentNoteTask = async () =>
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    someDocumentId,
                    someNoteId);

            // then
            await removeDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnRemoveIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int someDocumentId = GetRandomNumber();
            int someNoteId = GetRandomNumber();

            var unauthorizedDocumentNoteException =
                new UnauthorizedDocumentNoteException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedDependencyValidationException =
                new DocumentNoteDependencyValidationException(
                    message: "Document note dependency validation error occurred, "
                        + "fix the errors and try again.",
                    innerException: unauthorizedDocumentNoteException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<List<DocumentNote>>> removeDocumentNoteTask = async () =>
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    someDocumentId,
                    someNoteId);

            // then
            await removeDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(CriticalDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRemoveIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int someDocumentId = GetRandomNumber();
            int someNoteId = GetRandomNumber();

            var failedDocumentNoteDependencyException =
                new FailedDocumentNoteDependencyException(
                    message: "Failed document note dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedDependencyException =
                new DocumentNoteDependencyException(
                    message: "Document note dependency error occurred, contact support.",
                    innerException: failedDocumentNoteDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<List<DocumentNote>>> removeDocumentNoteTask = async () =>
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    someDocumentId,
                    someNoteId);

            // then
            await removeDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRemoveIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int someDocumentId = GetRandomNumber();
            int someNoteId = GetRandomNumber();

            var failedDocumentNoteDependencyException =
                new FailedDocumentNoteDependencyException(
                    message: "Failed document note dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDependencyException =
                new DocumentNoteDependencyException(
                    message: "Document note dependency error occurred, contact support.",
                    innerException: failedDocumentNoteDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<List<DocumentNote>>> removeDocumentNoteTask = async () =>
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    someDocumentId,
                    someNoteId);

            // then
            await removeDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRemoveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
            int someNoteId = GetRandomNumber();
            var serviceException = new Exception();

            var failedDocumentNoteServiceException =
                new FailedDocumentNoteServiceException(
                    message: "Failed document note service error occurred, contact support.",
                    innerException: serviceException);

            var expectedServiceException =
                new DocumentNoteServiceException(
                    message: "Document note service error occurred, contact support.",
                    innerException: failedDocumentNoteServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<List<DocumentNote>>> removeDocumentNoteTask = async () =>
                await this.documentNoteService.RemoveDocumentNoteAsync(
                    someDocumentId,
                    someNoteId);

            // then
            await removeDocumentNoteTask.Should()
                .ThrowAsync<DocumentNoteServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.DeleteDocumentNoteAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
