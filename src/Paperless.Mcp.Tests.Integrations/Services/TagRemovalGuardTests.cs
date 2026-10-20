using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using Paperless.Mcp.Models.Processings.Tags;
using Paperless.Mcp.Services.Foundations.Tags;
using Paperless.Mcp.Services.Processings.Tags;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Der confirm-Guard gegen die echte Instanz. Unit-Tests belegen, dass die Service
    // den Loeschaufruf unterlaesst; hier wird belegt, dass der Tag danach wirklich noch
    // da ist.
    [TestClass]
    public class TagRemovalGuardTests
    {
        private static (ITagService Foundation, ITagProcessingService Processing) CreateServices()
        {
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = new LoggingBroker(new NullLogger<LoggingBroker>());

            var tagService = new TagService(broker, loggingBroker);

            return (tagService, new TagProcessingService(tagService));
        }

        [TestMethod]
        public async Task ShouldLeaveTagInPlaceOnDryRunAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            (ITagService foundation, ITagProcessingService processing) = CreateServices();

            Tag addedTag = await foundation.AddTagAsync(new Tag
            {
                Name = $"zz-integrationstest-{Guid.NewGuid():N}"[..40],
            });

            try
            {
                // when
                TagRemovalPlan actualPlan =
                    await processing.RemoveTagByIdAsync(addedTag.Id, confirm: false);

                // then
                actualPlan.Removed.Should().BeFalse();
                actualPlan.Summary.Should().Contain("Dry run");

                // Der eigentliche Beweis: der Tag ist noch da
                Tag stillThere = await foundation.RetrieveTagByIdAsync(addedTag.Id);
                stillThere.Id.Should().Be(addedTag.Id);
            }
            finally
            {
                await IntegrationTestConfiguration.CleanUpAsync(
                    $"Tag {addedTag.Id}",
                    () => foundation.RemoveTagByIdAsync(addedTag.Id).AsTask());
            }
        }

        [TestMethod]
        public async Task ShouldRemoveTagWhenConfirmedAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            (ITagService foundation, ITagProcessingService processing) = CreateServices();

            Tag addedTag = await foundation.AddTagAsync(new Tag
            {
                Name = $"zz-integrationstest-{Guid.NewGuid():N}"[..40],
            });

            // when
            TagRemovalPlan actualPlan =
                await processing.RemoveTagByIdAsync(addedTag.Id, confirm: true);

            // then
            actualPlan.Removed.Should().BeTrue();

            // Und danach gibt es ihn nicht mehr
            Func<Task<Tag>> retrieveTask = async () =>
                await foundation.RetrieveTagByIdAsync(addedTag.Id);

            await retrieveTask.Should()
                .ThrowAsync<TagDependencyValidationException>();
        }

        [TestMethod]
        public async Task ShouldReportDocumentCountInThePlanAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            (ITagService foundation, ITagProcessingService processing) = CreateServices();

            Tag addedTag = await foundation.AddTagAsync(new Tag
            {
                Name = $"zz-integrationstest-{Guid.NewGuid():N}"[..40],
            });

            try
            {
                // when
                TagRemovalPlan actualPlan =
                    await processing.RemoveTagByIdAsync(addedTag.Id, confirm: false);

                // then
                // Ein frischer Tag haengt an keinem Dokument -- der Bericht muss das sagen,
                // denn genau diese Zahl macht eine Bestaetigung erst zu einer Entscheidung.
                actualPlan.DocumentCount.Should().Be(0);
                actualPlan.Summary.Should().Contain("No document");
            }
            finally
            {
                await IntegrationTestConfiguration.CleanUpAsync(
                    $"Tag {addedTag.Id}",
                    () => foundation.RemoveTagByIdAsync(addedTag.Id).AsTask());
            }
        }
    }
}
