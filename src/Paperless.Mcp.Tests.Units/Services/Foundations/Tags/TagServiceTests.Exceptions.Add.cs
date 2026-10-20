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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfBadRequestErrorOccursAndLogItAsync()
        {
            // given
            Tag someTag = CreateRandomTag();
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
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(httpResponseBadRequestException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfConflictErrorOccursAndLogItAsync()
        {
            // given
            Tag someTag = CreateRandomTag();
            var httpResponseConflictException = new HttpResponseConflictException();

            var alreadyExistsTagException =
                new AlreadyExistsTagException(
                    message: "Tag already exists.",
                    innerException: httpResponseConflictException);

            var expectedTagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: alreadyExistsTagException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(httpResponseConflictException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfNotFoundErrorOccursAndLogItAsync()
        {
            // given
            Tag someTag = CreateRandomTag();
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
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(httpResponseNotFoundException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
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
        public async Task ShouldThrowDependencyExceptionOnAddIfCriticalErrorOccursAndLogItAsync(
            Exception criticalDependencyException)
        {
            // given
            Tag someTag = CreateRandomTag();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: criticalDependencyException);

            var expectedTagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
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
        public async Task ShouldThrowDependencyExceptionOnAddIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            Tag someTag = CreateRandomTag();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedTagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyExceptionOnAddIfTransportErrorOccursAndLogItAsync()
        {
            // given
            Tag someTag = CreateRandomTag();
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
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(httpRequestException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedTagDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnAddIfServiceErrorOccursAndLogItAsync()
        {
            // given
            Tag someTag = CreateRandomTag();
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
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<Tag>> addTagTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagTask.Should()
                .ThrowAsync<TagServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
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
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            Tag someTag = CreateRandomTag();

            var unauthorizedTagException =
                new UnauthorizedTagException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedTagDependencyValidationException =
                new TagDependencyValidationException(
                    message: "Tag dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedTagException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<Tag>> addTagAsyncTask = async () =>
                await this.tagService.AddTagAsync(someTag);

            // then
            await addTagAsyncTask.Should()
                .ThrowAsync<TagDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedTagDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.PostTagAsync(It.IsAny<Tag>()),
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
