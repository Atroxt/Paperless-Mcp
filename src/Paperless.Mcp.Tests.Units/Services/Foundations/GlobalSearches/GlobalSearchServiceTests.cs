using System.Linq.Expressions;
using System.Net.Http;
using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.GlobalSearches;
using Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions;
using Paperless.Mcp.Services.Foundations.GlobalSearches;
using RESTFulSense.Exceptions;
using Tynamix.ObjectFiller;
using Xeptions;

namespace Paperless.Mcp.Tests.Units.Services.Foundations.GlobalSearches
{
    [TestClass]
    public class GlobalSearchServiceTests
    {
        private readonly Mock<IPaperlessBroker> paperlessBrokerMock;
        private readonly Mock<ILoggingBroker> loggingBrokerMock;
        private readonly IGlobalSearchService globalSearchService;

        public GlobalSearchServiceTests()
        {
            this.paperlessBrokerMock = new Mock<IPaperlessBroker>();
            this.loggingBrokerMock = new Mock<ILoggingBroker>();

            this.globalSearchService = new GlobalSearchService(
                paperlessBroker: this.paperlessBrokerMock.Object,
                loggingBroker: this.loggingBrokerMock.Object);
        }

        public static IEnumerable<object[]> InvalidTexts()
        {
            yield return [null!];
            yield return [string.Empty];
            yield return ["   "];
        }

        public static IEnumerable<object[]> UnauthorizedExceptions()
        {
            yield return [new HttpResponseUnauthorizedException()];
            yield return [new HttpResponseForbiddenException()];
        }

        public static IEnumerable<object[]> ServerDependencyExceptions()
        {
            yield return [new HttpResponseInternalServerErrorException()];
            yield return [new HttpResponseServiceUnavailableException()];
            yield return [new HttpResponseException()];
        }

        [TestMethod]
        public async Task ShouldMapTitlesAndNamesToLabelsAsync()
        {
            // given
            string someQuery = GetRandomName();

            var paperlessGlobalSearch = new PaperlessGlobalSearch
            {
                Total = 4,
                Documents = [new() { Id = 7, Title = "Rechnung 4711" }],
                Tags = [new() { Id = 8, Name = "Offen" }],
                Correspondents = [new() { Id = 9, Name = "Musterfirma" }],
                DocumentTypes = [new() { Id = 10, Name = "Rechnung" }],
                StoragePaths = [new() { Id = 11, Name = "Belege" }],
                CustomFields = [new() { Id = 12, Name = "Betrag" }],
            };

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetGlobalSearchAsync(someQuery))
                    .ReturnsAsync(paperlessGlobalSearch);

            // when
            GlobalSearchResult actualResult =
                await this.globalSearchService.RetrieveGlobalSearchAsync(someQuery);

            // then
            actualResult.Total.Should().Be(4);

            // Dokumente tragen title, alles andere name -- beides wird zu Label
            actualResult.Documents.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new GlobalSearchHit { Id = 7, Label = "Rechnung 4711" });

            actualResult.Tags.Should().ContainSingle()
                .Which.Should().BeEquivalentTo(new GlobalSearchHit { Id = 8, Label = "Offen" });

            actualResult.Correspondents.Should().ContainSingle()
                .Which.Label.Should().Be("Musterfirma");

            actualResult.DocumentTypes.Should().ContainSingle()
                .Which.Label.Should().Be("Rechnung");

            actualResult.StoragePaths.Should().ContainSingle()
                .Which.Label.Should().Be("Belege");

            actualResult.CustomFields.Should().ContainSingle()
                .Which.Label.Should().Be("Betrag");

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetGlobalSearchAsync(someQuery),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldReturnEmptyCollectionsWhenNothingMatchesAsync()
        {
            // given
            string someQuery = GetRandomName();

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetGlobalSearchAsync(someQuery))
                    .ReturnsAsync(new PaperlessGlobalSearch());

            // when
            GlobalSearchResult actualResult =
                await this.globalSearchService.RetrieveGlobalSearchAsync(someQuery);

            // then
            actualResult.Total.Should().Be(0);
            actualResult.Documents.Should().BeEmpty();
            actualResult.Tags.Should().BeEmpty();
            actualResult.Correspondents.Should().BeEmpty();
            actualResult.DocumentTypes.Should().BeEmpty();
            actualResult.StoragePaths.Should().BeEmpty();
            actualResult.CustomFields.Should().BeEmpty();

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetGlobalSearchAsync(someQuery),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(InvalidTexts))]
        public async Task ShouldThrowValidationExceptionIfQueryIsInvalidAndLogItAsync(
            string? invalidQuery)
        {
            // given
            var invalidGlobalSearchException =
                new InvalidGlobalSearchException(
                    message: "GlobalSearch is invalid, fix the errors and try again.");

            invalidGlobalSearchException.UpsertDataList(
                key: "Query",
                value: "Text is required");

            var expectedValidationException =
                new GlobalSearchValidationException(
                    message: "GlobalSearch validation error occurred, fix the errors and try again.",
                    innerException: invalidGlobalSearchException);

            // when
            Func<Task<GlobalSearchResult>> searchTask = async () =>
                await this.globalSearchService.RetrieveGlobalSearchAsync(invalidQuery!);

            // then
            await searchTask.Should()
                .ThrowAsync<GlobalSearchValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedValidationException));

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedValidationException))),
                    Times.Once);

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetGlobalSearchAsync(It.IsAny<string>()),
                    Times.Never);

            this.loggingBrokerMock.VerifyNoOtherCalls();
            this.paperlessBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(UnauthorizedExceptions))]
        public async Task ShouldThrowDependencyValidationExceptionIfTokenIsRejectedAndLogItAsync(
            Exception unauthorizedException)
        {
            // given
            string someQuery = GetRandomName();

            var unauthorizedGlobalSearchException =
                new UnauthorizedGlobalSearchException(
                    message: "Paperless rejected the API token.",
                    innerException: unauthorizedException);

            var expectedDependencyValidationException =
                new GlobalSearchDependencyValidationException(
                    message: "GlobalSearch dependency validation error occurred, fix the errors and try again.",
                    innerException: unauthorizedGlobalSearchException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetGlobalSearchAsync(It.IsAny<string>()))
                    .ThrowsAsync(unauthorizedException);

            // when
            Func<Task<GlobalSearchResult>> searchTask = async () =>
                await this.globalSearchService.RetrieveGlobalSearchAsync(someQuery);

            // then
            await searchTask.Should()
                .ThrowAsync<GlobalSearchDependencyValidationException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyValidationException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetGlobalSearchAsync(It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(
                    expectedDependencyValidationException))),
                        Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DynamicData(nameof(ServerDependencyExceptions))]
        public async Task ShouldThrowDependencyExceptionIfServerErrorOccursAndLogItAsync(
            Exception serverDependencyException)
        {
            // given
            string someQuery = GetRandomName();

            var failedDependencyException =
                new FailedGlobalSearchDependencyException(
                    message: "Failed globalSearch dependency error occurred, contact support.",
                    innerException: serverDependencyException);

            var expectedDependencyException =
                new GlobalSearchDependencyException(
                    message: "GlobalSearch dependency error occurred, contact support.",
                    innerException: failedDependencyException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetGlobalSearchAsync(It.IsAny<string>()))
                    .ThrowsAsync(serverDependencyException);

            // when
            Func<Task<GlobalSearchResult>> searchTask = async () =>
                await this.globalSearchService.RetrieveGlobalSearchAsync(someQuery);

            // then
            await searchTask.Should()
                .ThrowAsync<GlobalSearchDependencyException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedDependencyException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetGlobalSearchAsync(It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedDependencyException))),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowServiceExceptionIfServiceErrorOccursAndLogItAsync()
        {
            // given
            string someQuery = GetRandomName();
            var serviceException = new Exception();

            var failedServiceException =
                new FailedGlobalSearchServiceException(
                    message: "Failed globalSearch service error occurred, contact support.",
                    innerException: serviceException);

            var expectedServiceException =
                new GlobalSearchServiceException(
                    message: "GlobalSearch service error occurred, contact support.",
                    innerException: failedServiceException);

            this.paperlessBrokerMock.Setup(broker =>
                broker.GetGlobalSearchAsync(It.IsAny<string>()))
                    .ThrowsAsync(serviceException);

            // when
            Func<Task<GlobalSearchResult>> searchTask = async () =>
                await this.globalSearchService.RetrieveGlobalSearchAsync(someQuery);

            // then
            await searchTask.Should()
                .ThrowAsync<GlobalSearchServiceException>()
                .Where(actualException =>
                    actualException.SameExceptionAs(expectedServiceException));

            this.paperlessBrokerMock.Verify(broker =>
                broker.GetGlobalSearchAsync(It.IsAny<string>()),
                    Times.Once);

            this.loggingBrokerMock.Verify(broker =>
                broker.LogErrorAsync(It.Is(SameExceptionAs(expectedServiceException))),
                    Times.Once);

            this.paperlessBrokerMock.VerifyNoOtherCalls();
            this.loggingBrokerMock.VerifyNoOtherCalls();
        }

        private static string GetRandomName() =>
            new MnemonicString(wordCount: 1).GetValue();

        private static Expression<Func<Exception, bool>> SameExceptionAs(Xeption expectedException) =>
            actualException => ((Xeption)actualException).SameExceptionAs(expectedException);
    }
}
