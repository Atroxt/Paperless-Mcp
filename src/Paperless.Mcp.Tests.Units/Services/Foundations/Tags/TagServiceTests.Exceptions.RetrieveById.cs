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
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveByIdIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            int someTagId = GetRandomNumber();
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
                broker.GetTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<Tag>> retrieveTagByIdTask = async () =>
                await this.tagService.RetrieveTagByIdAsync(someTagId);

            // then
            await retrieveTagByIdTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveByIdIfTagIsNotFoundAndLogItAsync()
        {
            // given
            int someTagId = GetRandomNumber();
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
                broker.GetTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<Tag>> retrieveTagByIdTask = async () =>
                await this.tagService.RetrieveTagByIdAsync(someTagId);

            // then
            await retrieveTagByIdTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            int someTagId = GetRandomNumber();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedTagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<Tag>> retrieveTagByIdTask = async () =>
                await this.tagService.RetrieveTagByIdAsync(someTagId);

            // then
            await retrieveTagByIdTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            int someTagId = GetRandomNumber();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedTagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<Tag>> retrieveTagByIdTask = async () =>
                await this.tagService.RetrieveTagByIdAsync(someTagId);

            // then
            await retrieveTagByIdTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnRetrieveByIdIfTransportErrorOccursAndLogItAsync()
        {
            // given
            int someTagId = GetRandomNumber();
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
                broker.GetTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<Tag>> retrieveTagByIdTask = async () =>
                await this.tagService.RetrieveTagByIdAsync(someTagId);

            // then
            await retrieveTagByIdTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrieveByIdIfServiceErrorOccursAndLogItAsync()
        {
            // given
            int someTagId = GetRandomNumber();
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
                broker.GetTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<Tag>> retrieveTagByIdTask = async () =>
                await this.tagService.RetrieveTagByIdAsync(someTagId);

            // then
            await retrieveTagByIdTask.Should()
                .ThrowAsync<TagServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveByIdIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            int someTagId = GetRandomNumber();

            var unauthorizedTagException =
                new UnauthorizedTagException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedTagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedTagException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<Tag>> retrieveTagByIdAsyncTask = async () =>
                await this.tagService.RetrieveTagByIdAsync(someTagId);

            // then
            await retrieveTagByIdAsyncTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTagByIdAsync(It.IsAny<int>()),
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
