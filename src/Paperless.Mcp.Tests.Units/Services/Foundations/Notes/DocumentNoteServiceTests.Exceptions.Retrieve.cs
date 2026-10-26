using System.Net.Http;
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
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidDocumentNoteException =
                new InvalidDocumentNoteException(
                    message: "Document note is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedDependencyValidationException =
                new DocumentNoteDependencyValidationException(
                    message: "Document note dependency validation error occurred, "
                        + "fix the errors and try again.",
                    innerException: invalidDocumentNoteException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(someDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Eine echte 404 heisst "dieses Dokument gibt es nicht" und ist damit ein
        // Eingabefehler, kein Infrastrukturproblem.
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfDocumentIsNotFoundAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
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
                broker.GetDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(someDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int someDocumentId = GetRandomNumber();

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
                broker.GetDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(someDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRetrieveIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int someDocumentId = GetRandomNumber();

            var failedDocumentNoteDependencyException =
                new FailedDocumentNoteDependencyException(
                    message: "Failed document note dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedDependencyException =
                new DocumentNoteDependencyException(
                    message: "Document note dependency error occurred, contact support.",
                    innerException: failedDocumentNoteDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(someDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRetrieveIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int someDocumentId = GetRandomNumber();

            var failedDocumentNoteDependencyException =
                new FailedDocumentNoteDependencyException(
                    message: "Failed document note dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDependencyException =
                new DocumentNoteDependencyException(
                    message: "Document note dependency error occurred, contact support.",
                    innerException: failedDocumentNoteDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(someDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrieveIfServerIsUnreachableAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
            var httpRequestException = new HttpRequestException();

            var failedDocumentNoteDependencyException =
                new FailedDocumentNoteDependencyException(
                    message: "Failed document note dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedDependencyException =
                new DocumentNoteDependencyException(
                    message: "Document note dependency error occurred, contact support.",
                    innerException: failedDocumentNoteDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(someDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrieveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int someDocumentId = GetRandomNumber();
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
                broker.GetDocumentNotesAsync(It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<List<DocumentNote>>> retrieveDocumentNotesTask = async () =>
                await this.documentNoteService.RetrieveDocumentNotesAsync(someDocumentId);

            // then
            await retrieveDocumentNotesTask.Should()
                .ThrowAsync<DocumentNoteServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetDocumentNotesAsync(It.IsAny<int>()),
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
