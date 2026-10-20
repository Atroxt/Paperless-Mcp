using Moq;
using Paperless.Mcp.Brokers.DateTimes;
using Paperless.Mcp.Brokers.Delays;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Tasks;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Tasks;
using Paperless.Mcp.Services.Orchestrations.Documents;
using Tynamix.ObjectFiller;

namespace Paperless.Mcp.Tests.Units.Services.Orchestrations.Documents
{
    [TestClass]
    public partial class DocumentOrchestrationServiceTests
    {
        private readonly Mock<IDocumentService> documentServiceMock;
        private readonly Mock<ITaskService> taskServiceMock;
        private readonly Mock<IDelayBroker> delayBrokerMock;
        private readonly Mock<IDateTimeBroker> dateTimeBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IDocumentOrchestrationService documentOrchestrationService;

        public DocumentOrchestrationServiceTests()
        {
            this.documentServiceMock = new Mock<IDocumentService>();
            this.taskServiceMock = new Mock<ITaskService>();
            this.delayBrokerMock = new Mock<IDelayBroker>();
            this.dateTimeBrokerMock = new Mock<IDateTimeBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.documentOrchestrationService = new DocumentOrchestrationService(
                documentService: this.documentServiceMock.Object,
                taskService: this.taskServiceMock.Object,
                delayBroker: this.delayBrokerMock.Object,
                dateTimeBroker: this.dateTimeBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        // Die Uhr steht still, solange nichts anderes gesagt wird: dann laeuft die
        // Wartezeit nie ab und der Test haengt an dem, was er wirklich prueft.
        private void SetUpStandingClock() =>
            this.dateTimeBrokerMock.Setup(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(FixedNow);

        // Erst die Startzeit, danach eine Zeit nach Ablauf: so laeuft genau eine Runde.
        private void SetUpExpiringClock() =>
            this.dateTimeBrokerMock.SetupSequence(broker =>
                broker.GetCurrentDateTimeOffsetAsync())
                    .ReturnsAsync(FixedNow)
                    .ReturnsAsync(FixedNow.AddHours(1));

        private static PaperlessTask CreateTask(string status, params int[] documentIds) =>
            new()
            {
                TaskId = Guid.NewGuid().ToString(),
                Status = status,
                RelatedDocumentIds = [.. documentIds],
            };

        private static DocumentUpload CreateDocumentUpload() =>
            new()
            {
                Content = [1, 2, 3],
                FileName = $"{GetRandomName()}.pdf",
            };

        private static string GetRandomName() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static int GetRandomNumber() =>
            new IntRange(min: 1, max: 99).GetValue();

        private static readonly DateTimeOffset FixedNow =
            new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);
    }
}
