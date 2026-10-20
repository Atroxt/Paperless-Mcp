using System.Linq.Expressions;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Tasks;
using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;
using Paperless.Mcp.Services.Foundations.Tasks;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Tasks
{
    [TestClass]
    public class TaskServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly ITaskService taskService;

        public TaskServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.taskService = new TaskService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        public static IEnumerable<object[]> InvalidTexts()
        {
            yield return [null!];
            yield return [string.Empty];
            yield return ["   "];
        }

        [TestMethod]
        public async Task ShouldRetrieveTaskByTaskIdAsync()
        {
            // given
            string someTaskId = Guid.NewGuid().ToString();

            var storedTask = new PaperlessTask
            {
                Id = 1,
                TaskId = someTaskId,
                Status = "SUCCESS",
                RelatedDocumentIds = [42],
            };

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTaskPageByTaskIdAsync(someTaskId))
                    .ReturnsAsync(new PaperlessTaskPage { Count = 1, Results = [storedTask] });

            // when
            PaperlessTask actualTask =
                await this.taskService.RetrieveTaskByTaskIdAsync(someTaskId);

            // then
            // Paperless antwortet paginiert, der Dienst gibt den einen Eintrag.
            actualTask.Should().BeEquivalentTo(storedTask);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTaskPageByTaskIdAsync(someTaskId),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionIfTaskIsUnknownAndLogItAsync()
        {
            // given
            string unknownTaskId = Guid.NewGuid().ToString();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetTaskPageByTaskIdAsync(unknownTaskId))
                    .ReturnsAsync(new PaperlessTaskPage());

            var notFoundPaperlessTaskException =
                new NotFoundPaperlessTaskException(message: "PaperlessTask not found.");

            var expectedDependencyValidationException =
                new PaperlessTaskDependencyValidationException(
                    message: "PaperlessTask dependency validation error occurred, fix the errors and try again.",
                    innerException: notFoundPaperlessTaskException);

            // when
            Func<Task<PaperlessTask>> retrieveTask = async () =>
                await this.taskService.RetrieveTaskByTaskIdAsync(unknownTaskId);

            // then
            // Eine leere Seite ist kein Ausfall der Gegenstelle, sondern heisst:
            // diese Kennung kennt Paperless nicht.
            await retrieveTask.Should()
                .ThrowAsync<PaperlessTaskDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTaskPageByTaskIdAsync(unknownTaskId),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionIfTaskIdIsInvalidAndLogItAsync(
            string? invalidTaskId)
        {
            // given
            var invalidPaperlessTaskException =
                new InvalidPaperlessTaskException(
                    message: "PaperlessTask is invalid, fix the errors and try again.");

            invalidPaperlessTaskException.UpsertDataList(
                key: nameof(PaperlessTask.TaskId),
                value: "Text is required");

            var expectedValidationException =
                new PaperlessTaskValidationException(
                    message: "PaperlessTask validation error occurred, fix the errors and try again.",
                    innerException: invalidPaperlessTaskException);

            // when
            Func<Task<PaperlessTask>> retrieveTask = async () =>
                await this.taskService.RetrieveTaskByTaskIdAsync(invalidTaskId!);

            // then
            await retrieveTask.Should()
                .ThrowAsync<PaperlessTaskValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetTaskPageByTaskIdAsync(It.IsAny<string>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
