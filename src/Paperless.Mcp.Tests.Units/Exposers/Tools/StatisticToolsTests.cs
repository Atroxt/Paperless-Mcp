using AwesomeAssertions;
using ModelContextProtocol;
using Moq;
using Paperless.Mcp.Exposers.Tools;
using Paperless.Mcp.Models.Foundations.Statistics;
using Paperless.Mcp.Models.Foundations.Statistics.Exceptions;
using Paperless.Mcp.Services.Foundations.Statistics;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Exposers.Tools
{
    [TestClass]
    public class StatisticToolsTests
    {
        private readonly Mock<IStatisticService> statisticServiceMock;
        private readonly StatisticTools statisticTools;

        public StatisticToolsTests()
        {
            this.statisticServiceMock = new Mock<IStatisticService>();

            this.statisticTools = new StatisticTools(
                statisticService: this.statisticServiceMock.Object);
        }

        [TestMethod]
        public async Task ShouldGetStatisticsAsync()
        {
            // given
            var storedStatistic = new Statistic { DocumentsTotal = 7, DocumentsInbox = 3 };
            Statistic expectedStatistic = storedStatistic;

            this.statisticServiceMock.Setup(service =>
                service.RetrieveStatisticAsync())
                    .ReturnsAsync(storedStatistic);

            // when
            Statistic actualStatistic = await this.statisticTools.GetStatisticsAsync();

            // then
            actualStatistic.Should().BeSameAs(expectedStatistic);

            this.statisticServiceMock.Verify(service =>
                service.RetrieveStatisticAsync(),
                    Times.Once);

            this.statisticServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReportRejectedTokenOnGetStatisticsAsync()
        {
            // given
            var statisticDependencyValidationException =
                new StatisticDependencyValidationException(
                    message: "Statistic dependency validation error occurred, fix the errors and try again.",
                    innerException: new UnauthorizedStatisticException(
                        message: "Paperless rejected the API token.",
                        innerException: new Exception()));

            this.statisticServiceMock.Setup(service =>
                service.RetrieveStatisticAsync())
                    .ThrowsAsync(statisticDependencyValidationException);

            // when
            Func<Task<Statistic>> getStatisticsTask = async () =>
                await this.statisticTools.GetStatisticsAsync();

            // then
            await getStatisticsTask.Should()
                .ThrowAsync<McpException>()
                .WithMessage("Paperless rejected the API token.");
        }

        [TestMethod]
        public async Task ShouldReportUnreachableServerOnGetStatisticsAsync()
        {
            // given
            var statisticDependencyException =
                new StatisticDependencyException(
                    message: "Statistic dependency error occurred, contact support.",
                    innerException: new Xeption());

            this.statisticServiceMock.Setup(service =>
                service.RetrieveStatisticAsync())
                    .ThrowsAsync(statisticDependencyException);

            // when
            Func<Task<Statistic>> getStatisticsTask = async () =>
                await this.statisticTools.GetStatisticsAsync();

            // then
            await getStatisticsTask.Should()
                .ThrowAsync<McpException>()
                .WithMessage("The Paperless server could not be reached*");
        }
    }
}
