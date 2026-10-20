using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Foundations.GlobalSearches;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Services.Foundations.Correspondents;
using Paperless.Mcp.Services.Foundations.GlobalSearches;
using Paperless.Mcp.Services.Foundations.Tags;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    [TestClass]
    public class GlobalSearchServiceTests
    {
        private static ILoggingBroker CreateLoggingBroker() =>
            new LoggingBroker(new NullLogger<LoggingBroker>());

        [TestMethod]
        public async Task ShouldFindTagAndCorrespondentUnderOneQueryAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = CreateLoggingBroker();

            var tagService = new TagService(broker, loggingBroker);
            var correspondentService = new CorrespondentService(broker, loggingBroker);
            var globalSearchService = new GlobalSearchService(broker, loggingBroker);

            // Ein Stichwort, das es sonst nirgends gibt
            string marker = $"zzsuche{Guid.NewGuid():N}"[..20];

            Tag? addedTag = null;
            Correspondent? addedCorrespondent = null;

            try
            {
                addedTag = await tagService.AddTagAsync(new Tag { Name = $"{marker}-tag" });

                addedCorrespondent = await correspondentService.AddCorrespondentAsync(
                    new Correspondent { Name = $"{marker}-firma" });

                // when
                GlobalSearchResult actualResult =
                    await globalSearchService.RetrieveGlobalSearchAsync(marker);

                // then
                // Ein Aufruf findet Treffer in zwei verschiedenen Sammlungen
                actualResult.Tags.Should()
                    .Contain(hit => hit.Id == addedTag.Id && hit.Label == addedTag.Name);

                actualResult.Correspondents.Should()
                    .Contain(hit => hit.Id == addedCorrespondent.Id
                        && hit.Label == addedCorrespondent.Name);

                actualResult.Total.Should().BeGreaterThanOrEqualTo(2);
            }
            finally
            {
                if (addedTag is not null)
                {
                    await IntegrationTestConfiguration.CleanUpAsync(
                        $"Tag {addedTag.Id}",
                        () => tagService.RemoveTagByIdAsync(addedTag.Id).AsTask());
                }

                if (addedCorrespondent is not null)
                {
                    await IntegrationTestConfiguration.CleanUpAsync(
                        $"Korrespondent {addedCorrespondent.Id}",
                        () => correspondentService
                            .RemoveCorrespondentByIdAsync(addedCorrespondent.Id).AsTask());
                }
            }
        }

        [TestMethod]
        public async Task ShouldFindDocumentsWithLabelOnlyAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();

            var globalSearchService = new GlobalSearchService(broker, CreateLoggingBroker());

            // when
            GlobalSearchResult actualResult =
                await globalSearchService.RetrieveGlobalSearchAsync("rechnung");

            // then
            // Paperless liefert hier vollstaendige Dokumente samt Text; bei uns kommen
            // nur Id und Bezeichnung an.
            actualResult.Documents.Should().AllSatisfy(hit =>
            {
                hit.Id.Should().BeGreaterThan(0);
                hit.Label.Should().NotBeNullOrWhiteSpace();
            });
        }

        [TestMethod]
        public async Task ShouldReturnNothingForImpossibleQueryAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();

            var globalSearchService = new GlobalSearchService(broker, CreateLoggingBroker());

            // when
            GlobalSearchResult actualResult = await globalSearchService
                .RetrieveGlobalSearchAsync($"zzgibtesnicht{Guid.NewGuid():N}");

            // then
            actualResult.Total.Should().Be(0);
            actualResult.Documents.Should().BeEmpty();
            actualResult.Tags.Should().BeEmpty();
            actualResult.Correspondents.Should().BeEmpty();
        }
    }
}
