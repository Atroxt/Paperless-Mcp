using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Tasks;
using Paperless.Mcp.Models.Foundations.Tasks.Exceptions;
using Paperless.Mcp.Models.Orchestrations.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Orchestrations.Documents
{
    public partial class DocumentOrchestrationServiceTests
    {
        [TestMethod]
        public async Task ShouldUploadAndReturnDocumentAsync()
        {
            // given
            SetUpStandingClock();

            DocumentUpload inputUpload = CreateDocumentUpload();
            string randomTaskId = Guid.NewGuid().ToString();
            int randomDocumentId = GetRandomNumber();

            PaperlessTask finishedTask = CreateTask("success", randomDocumentId);

            var storedDocument = new Document
            {
                Id = randomDocumentId,
                Title = GetRandomName(),
            };

            this.documentServiceMock.Setup(service =>
                service.AddDocumentFileAsync(inputUpload))
                    .ReturnsAsync(randomTaskId);

            this.taskServiceMock.Setup(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId))
                    .ReturnsAsync(finishedTask);

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentByIdAsync(randomDocumentId))
                    .ReturnsAsync(storedDocument);

            // when
            DocumentUploadResult actualResult =
                await this.documentOrchestrationService.AddDocumentAndAwaitAsync(
                    inputUpload, timeoutSeconds: 30);

            // then
            actualResult.Completed.Should().BeTrue();
            actualResult.TaskId.Should().Be(randomTaskId);
            actualResult.DocumentId.Should().Be(randomDocumentId);
            actualResult.Document.Should().BeEquivalentTo(storedDocument);
            actualResult.Summary.Should().Contain(storedDocument.Title);

            this.documentServiceMock.Verify(service =>
                service.AddDocumentFileAsync(inputUpload),
                    Times.Once);

            this.taskServiceMock.Verify(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(randomDocumentId),
                    Times.Once);

            this.delayBrokerMock.Verify(broker =>
                broker.DelayAsync(It.IsAny<TimeSpan>()),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
            this.taskServiceMock.VerifyNoOtherCalls();
            this.delayBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Direkt nach dem Upload gibt es die Auftragszeile noch nicht. Das darf den
        // Ablauf nicht abbrechen -- es heisst nur: noch einmal fragen.
        [TestMethod]
        public async Task ShouldKeepWaitingWhileTheTaskIsNotVisibleYetAsync()
        {
            // given
            SetUpStandingClock();

            DocumentUpload inputUpload = CreateDocumentUpload();
            string randomTaskId = Guid.NewGuid().ToString();
            int randomDocumentId = GetRandomNumber();

            var storedDocument = new Document { Id = randomDocumentId, Title = GetRandomName() };

            this.documentServiceMock.Setup(service =>
                service.AddDocumentFileAsync(inputUpload))
                    .ReturnsAsync(randomTaskId);

            this.taskServiceMock.SetupSequence(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId))
                    .ThrowsAsync(new PaperlessTaskDependencyValidationException(
                        message: "PaperlessTask dependency validation error occurred, fix the errors and try again.",
                        innerException: new NotFoundPaperlessTaskException(
                            message: "PaperlessTask not found.")))
                    .ReturnsAsync(CreateTask("started"))
                    .ReturnsAsync(CreateTask("success", randomDocumentId));

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentByIdAsync(randomDocumentId))
                    .ReturnsAsync(storedDocument);

            // when
            DocumentUploadResult actualResult =
                await this.documentOrchestrationService.AddDocumentAndAwaitAsync(
                    inputUpload, timeoutSeconds: 30);

            // then
            actualResult.Completed.Should().BeTrue();
            actualResult.DocumentId.Should().Be(randomDocumentId);

            this.taskServiceMock.Verify(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId),
                    Times.Exactly(3));

            this.delayBrokerMock.Verify(broker =>
                broker.DelayAsync(It.IsAny<TimeSpan>()),
                    Times.Exactly(3));

            this.documentServiceMock.Verify(service =>
                service.AddDocumentFileAsync(inputUpload),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(randomDocumentId),
                    Times.Once);

            this.dateTimeBrokerMock.Verify(broker =>
                broker.GetCurrentDateTimeOffsetAsync(),
                    Times.Exactly(3));

            this.documentServiceMock.VerifyNoOtherCalls();
            this.taskServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Ein Fehlschlag ist Auskunft, kein Ausfall: der Aufrufer soll erfahren, dass
        // Paperless die Datei nicht verarbeiten konnte, nicht eine Exception sehen.
        [TestMethod]
        public async Task ShouldReportFailureWithoutThrowingAsync()
        {
            // given
            SetUpStandingClock();

            DocumentUpload inputUpload = CreateDocumentUpload();
            string randomTaskId = Guid.NewGuid().ToString();

            this.documentServiceMock.Setup(service =>
                service.AddDocumentFileAsync(inputUpload))
                    .ReturnsAsync(randomTaskId);

            this.taskServiceMock.Setup(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId))
                    .ReturnsAsync(CreateTask("failure"));

            // when
            DocumentUploadResult actualResult =
                await this.documentOrchestrationService.AddDocumentAndAwaitAsync(
                    inputUpload, timeoutSeconds: 30);

            // then
            actualResult.Completed.Should().BeTrue();
            actualResult.Status.Should().Be("failure");
            actualResult.DocumentId.Should().BeNull();
            actualResult.Summary.Should().Contain(randomTaskId);

            this.documentServiceMock.Verify(service =>
                service.AddDocumentFileAsync(inputUpload),
                    Times.Once);

            this.taskServiceMock.Verify(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId),
                    Times.Once);

            // Kein Nachlesen: es gibt kein Dokument.
            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
            this.taskServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Ein Upload, der laenger dauert als die Wartezeit, ist nicht verloren. Die
        // TaskId ist der Faden, an dem man ihn wiederfindet.
        [TestMethod]
        public async Task ShouldReportStillRunningWhenTimeoutIsReachedAsync()
        {
            // given
            SetUpExpiringClock();

            DocumentUpload inputUpload = CreateDocumentUpload();
            string randomTaskId = Guid.NewGuid().ToString();

            this.documentServiceMock.Setup(service =>
                service.AddDocumentFileAsync(inputUpload))
                    .ReturnsAsync(randomTaskId);

            this.taskServiceMock.Setup(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId))
                    .ReturnsAsync(CreateTask("started"));

            // when
            DocumentUploadResult actualResult =
                await this.documentOrchestrationService.AddDocumentAndAwaitAsync(
                    inputUpload, timeoutSeconds: 1);

            // then
            actualResult.Completed.Should().BeFalse();
            actualResult.Status.Should().Be("started");
            actualResult.TaskId.Should().Be(randomTaskId);
            actualResult.Summary.Should().Contain(randomTaskId);
            actualResult.Summary.Should().Contain("paperless_tasks_get");

            this.documentServiceMock.Verify(service =>
                service.AddDocumentFileAsync(inputUpload),
                    Times.Once);

            this.taskServiceMock.Verify(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
            this.taskServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        // Erfolg ohne Dokument-Id ist moeglich -- etwa wenn Paperless die Datei verworfen
        // hat. Ohne diesen Zweig liefe der Dienst in einen Zugriff auf ein leeres Feld.
        [TestMethod]
        public async Task ShouldReportSuccessWithoutDocumentIdAsync()
        {
            // given
            SetUpStandingClock();

            DocumentUpload inputUpload = CreateDocumentUpload();
            string randomTaskId = Guid.NewGuid().ToString();

            this.documentServiceMock.Setup(service =>
                service.AddDocumentFileAsync(inputUpload))
                    .ReturnsAsync(randomTaskId);

            this.taskServiceMock.Setup(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId))
                    .ReturnsAsync(CreateTask("success"));

            // when
            DocumentUploadResult actualResult =
                await this.documentOrchestrationService.AddDocumentAndAwaitAsync(
                    inputUpload, timeoutSeconds: 30);

            // then
            actualResult.Completed.Should().BeTrue();
            actualResult.DocumentId.Should().BeNull();
            actualResult.Summary.Should().Contain(randomTaskId);

            this.documentServiceMock.Verify(service =>
                service.AddDocumentFileAsync(inputUpload),
                    Times.Once);

            this.taskServiceMock.Verify(service =>
                service.RetrieveTaskByTaskIdAsync(randomTaskId),
                    Times.Once);

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentByIdAsync(It.IsAny<int>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
            this.taskServiceMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
