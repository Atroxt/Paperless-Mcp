using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;
using Paperless.Mcp.Services.Foundations.Correspondents;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    [TestClass]
    public class CorrespondentServiceTests
    {
        private static ICorrespondentService CreateCorrespondentService()
        {
            IPaperlessBroker paperlessBroker = IntegrationTestConfiguration.CreateBroker();

            ILoggingBroker loggingBroker =
                new LoggingBroker(new NullLogger<LoggingBroker>());

            return new CorrespondentService(paperlessBroker, loggingBroker);
        }

        private static string CreateTestCorrespondentName() =>
            $"zz-integrationstest-{Guid.NewGuid():N}"[..40];

        [TestMethod]
        public async Task ShouldWalkThroughCorrespondentLifecycleAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ICorrespondentService correspondentService = CreateCorrespondentService();

            var newCorrespondent = new Correspondent
            {
                Name = CreateTestCorrespondentName(),
                Match = "musterfirma",
                MatchingAlgorithm = CorrespondentMatchingAlgorithm.Any,
                IsInsensitive = true,
            };

            Correspondent? addedCorrespondent = null;

            try
            {
                // when
                addedCorrespondent =
                    await correspondentService.AddCorrespondentAsync(newCorrespondent);

                // then
                addedCorrespondent.Id.Should().BeGreaterThan(0);
                addedCorrespondent.Name.Should().Be(newCorrespondent.Name);
                addedCorrespondent.Match.Should().Be("musterfirma");
                addedCorrespondent.MatchingAlgorithm.Should()
                    .Be(CorrespondentMatchingAlgorithm.Any);

                // Von Paperless vergeben: belegt die snake_case-Zuordnung
                addedCorrespondent.Slug.Should().NotBeNullOrWhiteSpace();

                // when: einzeln lesen
                Correspondent retrievedCorrespondent =
                    await correspondentService.RetrieveCorrespondentByIdAsync(
                        addedCorrespondent.Id);

                // then
                retrievedCorrespondent.Id.Should().Be(addedCorrespondent.Id);
                retrievedCorrespondent.Name.Should().Be(newCorrespondent.Name);
                retrievedCorrespondent.DocumentCount.Should().Be(0);

                // last_correspondence ist null, solange kein Dokument zugeordnet ist --
                // belegt, dass das nullable Datumsfeld korrekt gelesen wird.
                retrievedCorrespondent.LastCorrespondence.Should().BeNull();
                retrievedCorrespondent.UserCanChange.Should().BeTrue();

                // when: aendern
                retrievedCorrespondent.Name = CreateTestCorrespondentName();
                retrievedCorrespondent.MatchingAlgorithm =
                    CorrespondentMatchingAlgorithm.Literal;

                Correspondent modifiedCorrespondent =
                    await correspondentService.ModifyCorrespondentAsync(retrievedCorrespondent);

                // then
                modifiedCorrespondent.Id.Should().Be(addedCorrespondent.Id);
                modifiedCorrespondent.Name.Should().Be(retrievedCorrespondent.Name);
                modifiedCorrespondent.MatchingAlgorithm.Should()
                    .Be(CorrespondentMatchingAlgorithm.Literal);

                // when: seitenweise lesen
                CorrespondentPage correspondentPage =
                    await correspondentService.RetrieveCorrespondentPageAsync(
                        page: 1, pageSize: 100);

                // then
                correspondentPage.Count.Should().BeGreaterThan(0);
                correspondentPage.Results.Should()
                    .Contain(correspondent => correspondent.Id == addedCorrespondent.Id);
            }
            finally
            {
                if (addedCorrespondent is not null)
                {
                    await correspondentService.RemoveCorrespondentByIdAsync(
                        addedCorrespondent.Id);
                }
            }
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfCorrespondentDoesNotExistAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ICorrespondentService correspondentService = CreateCorrespondentService();
            const int unlikelyCorrespondentId = 999_999_999;

            // when
            Func<Task<Correspondent>> retrieveCorrespondentTask = async () =>
                await correspondentService.RetrieveCorrespondentByIdAsync(unlikelyCorrespondentId);

            // then
            await retrieveCorrespondentTask.Should()
                .ThrowAsync<CorrespondentDependencyValidationException>();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfNameIsDuplicateAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ICorrespondentService correspondentService = CreateCorrespondentService();

            var firstCorrespondent = new Correspondent { Name = CreateTestCorrespondentName() };
            Correspondent? addedCorrespondent = null;

            try
            {
                addedCorrespondent =
                    await correspondentService.AddCorrespondentAsync(firstCorrespondent);

                var duplicateCorrespondent =
                    new Correspondent { Name = firstCorrespondent.Name };

                // when
                Func<Task<Correspondent>> addDuplicateTask = async () =>
                    await correspondentService.AddCorrespondentAsync(duplicateCorrespondent);

                // then
                await addDuplicateTask.Should()
                    .ThrowAsync<CorrespondentDependencyValidationException>();
            }
            finally
            {
                if (addedCorrespondent is not null)
                {
                    await correspondentService.RemoveCorrespondentByIdAsync(
                        addedCorrespondent.Id);
                }
            }
        }
    }
}
