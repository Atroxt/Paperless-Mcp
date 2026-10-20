using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using Paperless.Mcp.Services.Foundations.StoragePaths;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    [TestClass]
    public class StoragePathServiceTests
    {
        private static IStoragePathService CreateStoragePathService()
        {
            IPaperlessBroker paperlessBroker = IntegrationTestConfiguration.CreateBroker();

            ILoggingBroker loggingBroker =
                new LoggingBroker(new NullLogger<LoggingBroker>());

            return new StoragePathService(paperlessBroker, loggingBroker);
        }

        private static string CreateTestStoragePathName() =>
            $"zz-integrationstest-{Guid.NewGuid():N}"[..40];

        [TestMethod]
        public async Task ShouldWalkThroughStoragePathLifecycleAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IStoragePathService storagePathService = CreateStoragePathService();

            var newStoragePath = new StoragePath
            {
                Name = CreateTestStoragePathName(),
                Path = "{created_year}/{correspondent}/{title}",
                Match = "rechnung",
                MatchingAlgorithm = StoragePathMatchingAlgorithm.Any,
                IsInsensitive = true,
            };

            StoragePath? addedStoragePath = null;

            try
            {
                // when
                addedStoragePath =
                    await storagePathService.AddStoragePathAsync(newStoragePath);

                // then
                addedStoragePath.Id.Should().BeGreaterThan(0);
                addedStoragePath.Name.Should().Be(newStoragePath.Name);
                addedStoragePath.Match.Should().Be("rechnung");

                // Paperless schreibt {created_year} in seine eigene Notation
                // ({{ created_year }}) um, deshalb keine Pruefung auf Zeichengleichheit.
                addedStoragePath.Path.Should().NotBeNullOrWhiteSpace();
                addedStoragePath.Path.Should().Contain("created_year");
                addedStoragePath.Path.Should().Contain("title");
                addedStoragePath.MatchingAlgorithm.Should()
                    .Be(StoragePathMatchingAlgorithm.Any);

                // Von Paperless vergeben: belegt die snake_case-Zuordnung
                addedStoragePath.Slug.Should().NotBeNullOrWhiteSpace();

                // when: einzeln lesen
                StoragePath retrievedStoragePath =
                    await storagePathService.RetrieveStoragePathByIdAsync(
                        addedStoragePath.Id);

                // then
                retrievedStoragePath.Id.Should().Be(addedStoragePath.Id);
                retrievedStoragePath.Name.Should().Be(newStoragePath.Name);
                retrievedStoragePath.DocumentCount.Should().Be(0);
                retrievedStoragePath.UserCanChange.Should().BeTrue();

                // when: aendern
                retrievedStoragePath.Name = CreateTestStoragePathName();
                retrievedStoragePath.MatchingAlgorithm =
                    StoragePathMatchingAlgorithm.Literal;

                StoragePath modifiedStoragePath =
                    await storagePathService.ModifyStoragePathAsync(retrievedStoragePath);

                // then
                modifiedStoragePath.Id.Should().Be(addedStoragePath.Id);
                modifiedStoragePath.Name.Should().Be(retrievedStoragePath.Name);
                modifiedStoragePath.MatchingAlgorithm.Should()
                    .Be(StoragePathMatchingAlgorithm.Literal);

                // when: seitenweise lesen
                StoragePathPage storagePathPage =
                    await storagePathService.RetrieveStoragePathPageAsync(
                        page: 1, pageSize: 100);

                // then
                storagePathPage.Count.Should().BeGreaterThan(0);
                storagePathPage.Results.Should()
                    .Contain(storagePath => storagePath.Id == addedStoragePath.Id);
            }
            finally
            {
                if (addedStoragePath is not null)
                {
                    await storagePathService.RemoveStoragePathByIdAsync(
                        addedStoragePath.Id);
                }
            }
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfStoragePathDoesNotExistAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IStoragePathService storagePathService = CreateStoragePathService();
            const int unlikelyStoragePathId = 999_999_999;

            // when
            Func<Task<StoragePath>> retrieveStoragePathTask = async () =>
                await storagePathService.RetrieveStoragePathByIdAsync(unlikelyStoragePathId);

            // then
            await retrieveStoragePathTask.Should()
                .ThrowAsync<StoragePathDependencyValidationException>();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfNameIsDuplicateAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IStoragePathService storagePathService = CreateStoragePathService();

            var firstStoragePath = new StoragePath
            {
                Name = CreateTestStoragePathName(),
                Path = "{created_year}/{title}",
            };
            StoragePath? addedStoragePath = null;

            try
            {
                addedStoragePath =
                    await storagePathService.AddStoragePathAsync(firstStoragePath);

                var duplicateStoragePath =
                    new StoragePath
                    {
                        Name = firstStoragePath.Name,
                        Path = firstStoragePath.Path,
                    };

                // when
                Func<Task<StoragePath>> addDuplicateTask = async () =>
                    await storagePathService.AddStoragePathAsync(duplicateStoragePath);

                // then
                await addDuplicateTask.Should()
                    .ThrowAsync<StoragePathDependencyValidationException>();
            }
            finally
            {
                if (addedStoragePath is not null)
                {
                    await storagePathService.RemoveStoragePathByIdAsync(
                        addedStoragePath.Id);
                }
            }
        }
    }
}
