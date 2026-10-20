using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tags
{
    public partial class TagServiceTests
    {
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpResponseBadRequestException = new HttpResponseBadRequestException();

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.",
                    innerException: httpResponseBadRequestException);

            var expectedTagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(somePage, somePageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Paperless antwortet mit 404, wenn die angefragte Seite ausserhalb des Bereichs liegt.
        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfPageIsOutOfRangeAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpResponseNotFoundException = new HttpResponseNotFoundException();

            var notFoundTagException =
                new NotFoundTagException(
                    message: "Tag not found.",
                    innerException: httpResponseNotFoundException);

            var expectedTagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundTagException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(somePage, somePageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(CriticalDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedTagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(somePage, somePageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedTagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(somePage, somePageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrievePageIfTransportErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var httpRequestException = new HttpRequestException();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: httpRequestException);

            var expectedTagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(somePage, somePageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrievePageIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var serviceException = new Exception();

            var failedTagServiceException =
                new FailedTagServiceException(
                    message: "Failed tag service error occurred, contact support.",
                    innerException: serviceException);

            var expectedTagServiceException =
                new TagServiceException(
                    message: "Tag service error occurred, contact support.",
                    innerException: failedTagServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<TagPage>> retrieveTagPageTask = async () =>
                await this.tagService.RetrieveTagPageAsync(somePage, somePageSize);

            // then
            await retrieveTagPageTask.Should()
                .ThrowAsync<TagServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrievePageIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var unauthorizedTagException =
                new UnauthorizedTagException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedTagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedTagException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<TagPage>> retrieveTagPageAsyncTask = async () =>
                await this.tagService.RetrieveTagPageAsync(somePage, somePageSize);

            // then
            await retrieveTagPageAsyncTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

    }
}
