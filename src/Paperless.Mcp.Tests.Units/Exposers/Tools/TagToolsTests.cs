using AwesomeAssertions;
using ModelContextProtocol;
using Moq;
using Paperless.Mcp.Exposers.Tools;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using Paperless.Mcp.Services.Foundations.Tags;
using Paperless.Mcp.Services.Processings.Tags;
using Tynamix.ObjectFiller;

namespace Paperless.Mcp.Tests.Units.Exposers.Tools
{
    [TestClass]
    public class TagToolsTests
    {
        private readonly Mock<ITagService> tagServiceMock;
        private readonly Mock<ITagProcessingService> tagProcessingServiceMock;
        private readonly TagTools tagTools;

        public TagToolsTests()
        {
            this.tagServiceMock = new Mock<ITagService>();
            this.tagProcessingServiceMock = new Mock<ITagProcessingService>();

            this.tagTools = new TagTools(
                tagService: this.tagServiceMock.Object,
                tagProcessingService: this.tagProcessingServiceMock.Object);
        }

        [TestMethod]
        public async Task ShouldListTagsAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();
            var storedTagPage = new TagPage { Count = somePage };
            TagPage expectedTagPage = storedTagPage;

            this.tagServiceMock.Setup(service =>
                service.RetrieveTagPageAsync(somePage, somePageSize))
                    .ReturnsAsync(storedTagPage);

            // when
            TagPage actualTagPage =
                await this.tagTools.ListTagsAsync(somePage, somePageSize);

            // then
            actualTagPage.Should().BeSameAs(expectedTagPage);

            this.tagServiceMock.Verify(service =>
                service.RetrieveTagPageAsync(somePage, somePageSize),
                    Times.Once);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldCreateTagAsync()
        {
            // given
            string someName = GetRandomName();
            var createdTag = new Tag { Name = someName };

            this.tagServiceMock.Setup(service =>
                service.AddTagAsync(It.Is<Tag>(tag => tag.Name == someName)))
                    .ReturnsAsync(createdTag);

            // when
            Tag actualTag = await this.tagTools.CreateTagAsync(name: someName);

            // then
            actualTag.Should().BeSameAs(createdTag);

            this.tagServiceMock.Verify(service =>
                service.AddTagAsync(It.Is<Tag>(tag =>
                    tag.Name == someName
                    && tag.Id == 0)),
                        Times.Once);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        // Dieser Fall war beim Durchstich kaputt: die Regelverstoesse liegen als
        // List<string> in Xeption.Data, nicht als string[].
        [TestMethod]
        public async Task ShouldReportFieldErrorsOnListIfValidationErrorOccursAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            invalidTagException.UpsertDataList(
                key: "Page",
                value: "Value must be greater than zero");

            var tagValidationException =
                new TagValidationException(
                    message: "Tag validation error occurred, fix the errors and try again.",
                    innerException: invalidTagException);

            this.tagServiceMock.Setup(service =>
                service.RetrieveTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(tagValidationException);

            // when
            Func<Task<TagPage>> listTagsTask = async () =>
                await this.tagTools.ListTagsAsync(somePage, somePageSize);

            // then
            await listTagsTask.Should()
                .ThrowAsync<McpException>()
                .Where(actualException =>
                    actualException.Message.Contains("Tag is invalid")
                    && actualException.Message.Contains("Page")
                    && actualException.Message.Contains("Value must be greater than zero"));

            this.tagServiceMock.Verify(service =>
                service.RetrieveTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportGenericErrorOnListIfDependencyErrorOccursAsync()
        {
            // given
            int somePage = GetRandomNumber();
            int somePageSize = GetRandomNumber();

            var failedTagDependencyException =
                new FailedTagDependencyException(
                    message: "Failed tag dependency error occurred, contact support.",
                    innerException: new Exception());

            var tagDependencyException =
                new TagDependencyException(
                    message: "Tag dependency error occurred, contact support.",
                    innerException: failedTagDependencyException);

            this.tagServiceMock.Setup(service =>
                service.RetrieveTagPageAsync(It.IsAny<int>(), It.IsAny<int>()))
                    .ThrowsAsync(tagDependencyException);

            // when
            Func<Task<TagPage>> listTagsTask = async () =>
                await this.tagTools.ListTagsAsync(somePage, somePageSize);

            // then
            // Interna bleiben drinnen: der Aufrufer erfaehrt nur, dass der Server nicht antwortet.
            await listTagsTask.Should()
                .ThrowAsync<McpException>()
                .Where(actualException =>
                    actualException.Message.Contains("could not be reached")
                    && actualException.Message.Contains("contact support") == false);

            this.tagServiceMock.Verify(service =>
                service.RetrieveTagPageAsync(It.IsAny<int>(), It.IsAny<int>()),
                    Times.Once);

            this.tagServiceMock.VerifyNoOtherCalls();
        }

        private static string GetRandomName() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();
    }
}
