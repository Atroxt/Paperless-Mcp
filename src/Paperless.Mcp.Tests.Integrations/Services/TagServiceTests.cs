using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using Paperless.Mcp.Services.Foundations.Tags;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Prueft gegen eine echte Instanz, was Unit-Tests nicht koennen: ob die JSON-Form
    // stimmt. snake_case-Zuordnung, Enum-Werte, Pagination und die Uebersetzung echter
    // HTTP-Fehler in unsere Exception-Kategorien.
    [TestClass]
    public class TagServiceTests
    {
        private static ITagService CreateTagService()
        {
            IPaperlessBroker paperlessBroker = IntegrationTestConfiguration.CreateBroker();

            ILoggingBroker loggingBroker =
                new LoggingBroker(new NullLogger<LoggingBroker>());

            return new TagService(paperlessBroker, loggingBroker);
        }

        // Eindeutig als Testdatum erkennbar, damit versehentlich zurueckgebliebene
        // Eintraege in der Instanz sofort zuzuordnen sind.
        private static string CreateTestTagName() =>
            $"zz-integrationstest-{Guid.NewGuid():N}"[..40];

        [TestMethod]
        public async Task ShouldWalkThroughTagLifecycleAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ITagService tagService = CreateTagService();

            var newTag = new Tag
            {
                Name = CreateTestTagName(),
                Color = "#a1b2c3",
                Match = "rechnung",
                MatchingAlgorithm = TagMatchingAlgorithm.Any,
                IsInsensitive = true,
                IsInboxTag = false,
            };

            Tag? addedTag = null;

            try
            {
                // when
                addedTag = await tagService.AddTagAsync(newTag);

                // then
                addedTag.Id.Should().BeGreaterThan(0);
                addedTag.Name.Should().Be(newTag.Name);
                addedTag.Color.Should().Be("#a1b2c3");
                addedTag.MatchingAlgorithm.Should().Be(TagMatchingAlgorithm.Any);
                addedTag.Match.Should().Be("rechnung");

                // Von Paperless vergebene Felder: belegt die snake_case-Zuordnung
                addedTag.Slug.Should().NotBeNullOrWhiteSpace();

                // when: einzeln lesen
                Tag retrievedTag = await tagService.RetrieveTagByIdAsync(addedTag.Id);

                // then
                retrievedTag.Id.Should().Be(addedTag.Id);
                retrievedTag.Name.Should().Be(newTag.Name);
                retrievedTag.MatchingAlgorithm.Should().Be(TagMatchingAlgorithm.Any);

                // when: aendern
                retrievedTag.Name = CreateTestTagName();
                retrievedTag.MatchingAlgorithm = TagMatchingAlgorithm.Literal;
                Tag modifiedTag = await tagService.ModifyTagAsync(retrievedTag);

                // then
                modifiedTag.Id.Should().Be(addedTag.Id);
                modifiedTag.Name.Should().Be(retrievedTag.Name);
                modifiedTag.MatchingAlgorithm.Should().Be(TagMatchingAlgorithm.Literal);

                // when: seitenweise lesen
                TagPage tagPage = await tagService.RetrieveTagPageAsync(page: 1, pageSize: 100);

                // then
                tagPage.Count.Should().BeGreaterThan(0);
                tagPage.Results.Should().NotBeEmpty();
                tagPage.Results.Should().Contain(tag => tag.Id == addedTag.Id);
            }
            finally
            {
                if (addedTag is not null)
                {
                    await tagService.RemoveTagByIdAsync(addedTag.Id);
                }
            }
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfTagDoesNotExistAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ITagService tagService = CreateTagService();
            const int unlikelyTagId = 999_999_999;

            // when
            Func<Task<Tag>> retrieveTagTask = async () =>
                await tagService.RetrieveTagByIdAsync(unlikelyTagId);

            // then
            // Belegt die Zuordnung aus der Tabelle in README.md: eine echte 404 auf
            // eine Ressource ist Abhaengigkeitsvalidierung, nicht kritisch.
            await retrieveTagTask.Should()
                .ThrowAsync<TagDependencyValidationException>();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfNameIsDuplicateAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ITagService tagService = CreateTagService();

            var firstTag = new Tag { Name = CreateTestTagName() };
            Tag? addedTag = null;

            try
            {
                addedTag = await tagService.AddTagAsync(firstTag);

                var duplicateTag = new Tag { Name = firstTag.Name };

                // when
                Func<Task<Tag>> addDuplicateTask = async () =>
                    await tagService.AddTagAsync(duplicateTag);

                // then
                // Paperless weist einen doppelten Namen ab. Welcher Statuscode dabei
                // kommt (400 oder 409), laeuft in beiden Faellen auf dieselbe Kategorie.
                await addDuplicateTask.Should()
                    .ThrowAsync<TagDependencyValidationException>();
            }
            finally
            {
                if (addedTag is not null)
                {
                    await tagService.RemoveTagByIdAsync(addedTag.Id);
                }
            }
        }
    }
}
