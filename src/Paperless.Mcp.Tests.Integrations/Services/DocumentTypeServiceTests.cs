using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.DocumentTypes;
using Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions;
using Paperless.Mcp.Services.Foundations.DocumentTypes;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    [TestClass]
    public class DocumentTypeServiceTests
    {
        private static IDocumentTypeService CreateDocumentTypeService()
        {
            IPaperlessBroker paperlessBroker = IntegrationTestConfiguration.CreateBroker();

            ILoggingBroker loggingBroker =
                new LoggingBroker(new NullLogger<LoggingBroker>());

            return new DocumentTypeService(paperlessBroker, loggingBroker);
        }

        private static string CreateTestDocumentTypeName() =>
            $"zz-integrationstest-{Guid.NewGuid():N}"[..40];

        [TestMethod]
        public async Task ShouldWalkThroughDocumentTypeLifecycleAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentTypeService documentTypeService = CreateDocumentTypeService();

            var newDocumentType = new DocumentType
            {
                Name = CreateTestDocumentTypeName(),
                Match = "rechnung",
                MatchingAlgorithm = DocumentTypeMatchingAlgorithm.Any,
                IsInsensitive = true,
            };

            DocumentType? addedDocumentType = null;

            try
            {
                // when
                addedDocumentType =
                    await documentTypeService.AddDocumentTypeAsync(newDocumentType);

                // then
                addedDocumentType.Id.Should().BeGreaterThan(0);
                addedDocumentType.Name.Should().Be(newDocumentType.Name);
                addedDocumentType.Match.Should().Be("rechnung");
                addedDocumentType.MatchingAlgorithm.Should()
                    .Be(DocumentTypeMatchingAlgorithm.Any);

                // Von Paperless vergeben: belegt die snake_case-Zuordnung
                addedDocumentType.Slug.Should().NotBeNullOrWhiteSpace();

                // when: einzeln lesen
                DocumentType retrievedDocumentType =
                    await documentTypeService.RetrieveDocumentTypeByIdAsync(
                        addedDocumentType.Id);

                // then
                retrievedDocumentType.Id.Should().Be(addedDocumentType.Id);
                retrievedDocumentType.Name.Should().Be(newDocumentType.Name);
                retrievedDocumentType.DocumentCount.Should().Be(0);
                retrievedDocumentType.UserCanChange.Should().BeTrue();

                // when: aendern
                retrievedDocumentType.Name = CreateTestDocumentTypeName();
                retrievedDocumentType.MatchingAlgorithm =
                    DocumentTypeMatchingAlgorithm.Literal;

                DocumentType modifiedDocumentType =
                    await documentTypeService.ModifyDocumentTypeAsync(retrievedDocumentType);

                // then
                modifiedDocumentType.Id.Should().Be(addedDocumentType.Id);
                modifiedDocumentType.Name.Should().Be(retrievedDocumentType.Name);
                modifiedDocumentType.MatchingAlgorithm.Should()
                    .Be(DocumentTypeMatchingAlgorithm.Literal);

                // when: seitenweise lesen
                DocumentTypePage documentTypePage =
                    await documentTypeService.RetrieveDocumentTypePageAsync(
                        page: 1, pageSize: 100);

                // then
                documentTypePage.Count.Should().BeGreaterThan(0);
                documentTypePage.Results.Should()
                    .Contain(documentType => documentType.Id == addedDocumentType.Id);
            }
            finally
            {
                if (addedDocumentType is not null)
                {
                    await documentTypeService.RemoveDocumentTypeByIdAsync(
                        addedDocumentType.Id);
                }
            }
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfDocumentTypeDoesNotExistAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentTypeService documentTypeService = CreateDocumentTypeService();
            const int unlikelyDocumentTypeId = 999_999_999;

            // when
            Func<Task<DocumentType>> retrieveDocumentTypeTask = async () =>
                await documentTypeService.RetrieveDocumentTypeByIdAsync(unlikelyDocumentTypeId);

            // then
            await retrieveDocumentTypeTask.Should()
                .ThrowAsync<DocumentTypeDependencyValidationException>();
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnAddIfNameIsDuplicateAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IDocumentTypeService documentTypeService = CreateDocumentTypeService();

            var firstDocumentType = new DocumentType { Name = CreateTestDocumentTypeName() };
            DocumentType? addedDocumentType = null;

            try
            {
                addedDocumentType =
                    await documentTypeService.AddDocumentTypeAsync(firstDocumentType);

                var duplicateDocumentType =
                    new DocumentType { Name = firstDocumentType.Name };

                // when
                Func<Task<DocumentType>> addDuplicateTask = async () =>
                    await documentTypeService.AddDocumentTypeAsync(duplicateDocumentType);

                // then
                await addDuplicateTask.Should()
                    .ThrowAsync<DocumentTypeDependencyValidationException>();
            }
            finally
            {
                if (addedDocumentType is not null)
                {
                    await documentTypeService.RemoveDocumentTypeByIdAsync(
                        addedDocumentType.Id);
                }
            }
        }
    }
}
