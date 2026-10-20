using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Orchestrations.Documents;
using Paperless.Mcp.Models.Orchestrations.Documents.Exceptions;
using Paperless.Mcp.Services.Orchestrations.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Orchestrations.Documents
{
    public partial class DocumentOrchestrationServiceTests
    {
        public static IEnumerable<object[]> InvalidTimeouts()
        {
            yield return [0];
            yield return [-1];
            yield return [DocumentOrchestrationService.MaxTimeoutSeconds + 1];
        }

        // Die Obergrenze ist keine Schikane: ein Werkzeugaufruf, der minutenlang haengt,
        // blockiert den Assistenten, und der Auftrag laeuft auch ohne uns weiter.
        [TestMethod]
        [DynamicData(nameof(InvalidTimeouts))]
        public async Task ShouldThrowValidationExceptionIfTimeoutIsInvalidAndLogItAsync(
            int invalidTimeoutSeconds)
        {
            // given
            DocumentUpload inputUpload = CreateDocumentUpload();

            // when
            Func<Task<DocumentUploadResult>> uploadTask = async () =>
                await this.documentOrchestrationService.AddDocumentAndAwaitAsync(
                    inputUpload, invalidTimeoutSeconds);

            // then
            await uploadTask.Should()
                .ThrowAsync<DocumentOrchestrationValidationException>();

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.IsAny<Exception>()),
                    Times.Once);

            // Nichts darf hochgeladen worden sein, bevor die Wartezeit geprueft wurde.
            this.documentServiceMock.Verify(service =>
                service.AddDocumentFileAsync(It.IsAny<DocumentUpload>()),
                    Times.Never);

            this.documentServiceMock.VerifyNoOtherCalls();
            this.taskServiceMock.VerifyNoOtherCalls();
            this.delayBrokerMock.VerifyNoOtherCalls();
            this.dateTimeBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }
    }
}
