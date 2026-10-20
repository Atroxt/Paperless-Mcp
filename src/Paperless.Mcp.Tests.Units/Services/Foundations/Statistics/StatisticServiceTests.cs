using System.Linq.Expressions;
using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Statistics;
using Paperless.Mcp.Models.Foundations.Statistics.Exceptions;
using Paperless.Mcp.Services.Foundations.Statistics;
using RESTFulSense.Exceptions;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.Statistics
{
    [TestClass]
    public class StatisticServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IStatisticService statisticService;

        public StatisticServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.statisticService = new StatisticService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        public static IEnumerable<object[]> UnauthorizedExceptions()
        {
            yield return [new HttpResponseUnauthorizedException()];
            yield return [new HttpResponseForbiddenException()];
        }

        public static IEnumerable<object[]> CriticalDependencyExceptions()
        {
            yield return [new HttpResponseUrlNotFoundException()];
            yield return [new HttpRequestException()];
        }

        public static IEnumerable<object[]> ServerDependencyExceptions()
        {
            yield return [new HttpResponseInternalServerErrorException()];
            yield return [new HttpResponseServiceUnavailableException()];
            yield return [new HttpResponseException()];
        }

        [TestMethod]
        public async Task ShouldRetrieveStatisticAsync()
        {
            // given
            Statistic randomStatistic = CreateRandomStatistic();
            Statistic storedStatistic = randomStatistic;
            Statistic expectedStatistic = storedStatistic;

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatisticAsync())
                    .ReturnsAsync(storedStatistic);

            // when
            Statistic actualStatistic =
                await this.statisticService.RetrieveStatisticAsync();

            // then
            actualStatistic.Should().BeEquivalentTo(expectedStatistic);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatisticAsync(),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfUnauthorizedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            var unauthorizedStatisticException =
                new UnauthorizedStatisticException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedStatisticDependencyValidationException =
                new StatisticDependencyValidationException(
                    message: "Statistic dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedStatisticException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatisticAsync())
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<Statistic>> retrieveStatisticTask = async () =>
                await this.statisticService.RetrieveStatisticAsync();

            // then
            await retrieveStatisticTask.Should()
                .ThrowAsync<StatisticDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStatisticDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatisticAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStatisticDependencyValidationException))),
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
            StatisticDependencyException expectedStatisticDependencyException =
                CreateDependencyException(criticalDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatisticAsync())
                    .ThrowsAsync(criticalDependencyException);

            // when
            Func<Task<Statistic>> retrieveStatisticTask = async () =>
                await this.statisticService.RetrieveStatisticAsync();

            // then
            await retrieveStatisticTask.Should()
                .ThrowAsync<StatisticDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStatisticDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatisticAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogCriticalAsync(It.Is(SameExceptionAs(
                    expectedStatisticDependencyException))),
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
            StatisticDependencyException expectedStatisticDependencyException =
                CreateDependencyException(serverDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatisticAsync())
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<Statistic>> retrieveStatisticTask = async () =>
                await this.statisticService.RetrieveStatisticAsync();

            // then
            await retrieveStatisticTask.Should()
                .ThrowAsync<StatisticDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStatisticDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatisticAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStatisticDependencyException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionOnRetrieveIfServiceErrorOccursAndLogItAsync()
        {
            // given
            var serviceException = new Exception();

            var failedStatisticServiceException =
                new FailedStatisticServiceException(
                    message: "Failed statistic service error occurred, contact support.",
                    innerException: serviceException);

            var expectedStatisticServiceException =
                new StatisticServiceException(
                    message: "Statistic service error occurred, contact support.",
                    innerException: failedStatisticServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetStatisticAsync())
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<Statistic>> retrieveStatisticTask = async () =>
                await this.statisticService.RetrieveStatisticAsync();

            // then
            await retrieveStatisticTask.Should()
                .ThrowAsync<StatisticServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedStatisticServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetStatisticAsync(),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedStatisticServiceException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static StatisticDependencyException CreateDependencyException(Exception exception)
        {
            var failedStatisticDependencyException =
                new FailedStatisticDependencyException(
                    message: "Failed statistic dependency error occurred, contact support.",
                    innerException: exception);

            return new StatisticDependencyException(
                message: "Statistic dependency error occurred, contact support.",
                innerException: failedStatisticDependencyException);
        }

        private static Statistic CreateRandomStatistic() =>
            new()
            {
                DocumentsTotal = Random.Shared.Next(1, 1000),
                DocumentsInbox = Random.Shared.Next(1, 100),
                InboxTags = [Random.Shared.Next(1, 100)],
                DocumentFileTypeCounts =
                [
                    new StatisticFileTypeCount
                    {
                        MimeType = "application/pdf",
                        MimeTypeCount = Random.Shared.Next(1, 1000),
                    },
                ],
                CharacterCount = Random.Shared.Next(1, 100000),
                TagCount = Random.Shared.Next(1, 100),
                CorrespondentCount = Random.Shared.Next(1, 100),
                DocumentTypeCount = Random.Shared.Next(1, 100),
                StoragePathCount = Random.Shared.Next(1, 100),
                CurrentAsn = Random.Shared.Next(1, 100),
            };

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
