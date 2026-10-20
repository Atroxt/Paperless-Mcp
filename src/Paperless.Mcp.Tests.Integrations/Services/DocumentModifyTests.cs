using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Services.Foundations.CustomFields;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Foundations.Tags;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Diese Tests aendern ein echtes Dokument. Jeder stellt den Ausgangszustand im
    // finally-Zweig wieder her, damit die Reihenfolge der Tests keine Rolle spielt.
    [TestClass]
    public class DocumentModifyTests
    {
        private static ILoggingBroker CreateLoggingBroker() =>
            new LoggingBroker(new NullLogger<LoggingBroker>());

        private static async Task<Document> RequireAnyDocumentAsync(
            IDocumentService documentService)
        {
            DocumentPage page = await documentService.RetrieveDocumentPageAsync(
                new DocumentSearch { Page = 1, PageSize = 1 });

            if (page.Results.Count is 0)
            {
                Assert.Inconclusive(
                    "Uebersprungen: die Testinstanz enthaelt kein Dokument.");
            }

            return page.Results[0];
        }

        [TestMethod]
        public async Task ShouldChangeOnlyTheGivenFieldAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            var documentService = new DocumentService(broker, CreateLoggingBroker());

            Document original = await RequireAnyDocumentAsync(documentService);
            string originalTitle = original.Title;

            try
            {
                // when
                Document changed = await documentService.ModifyDocumentAsync(
                    new DocumentUpdate
                    {
                        DocumentId = original.Id,
                        Title = $"{originalTitle}-geaendert",
                    });

                // then
                changed.Title.Should().Be($"{originalTitle}-geaendert");

                // Der extrahierte Text bleibt unangetastet -- das ist der Grund fuer
                // PATCH statt PUT.
                changed.Content.Should().Be(original.Content);
                changed.CustomFields.Should().BeEquivalentTo(original.CustomFields);
            }
            finally
            {
                await documentService.ModifyDocumentAsync(new DocumentUpdate
                {
                    DocumentId = original.Id,
                    Title = originalTitle,
                });
            }
        }

        [TestMethod]
        public async Task ShouldAssignAndRemoveTagsAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = CreateLoggingBroker();

            var documentService = new DocumentService(broker, loggingBroker);
            var tagService = new TagService(broker, loggingBroker);

            Document original = await RequireAnyDocumentAsync(documentService);
            List<int> originalTags = [.. original.Tags];

            Tag? addedTag = null;

            try
            {
                addedTag = await tagService.AddTagAsync(new Tag
                {
                    Name = $"zz-integrationstest-{Guid.NewGuid():N}"[..40],
                });

                // when: Tag zuweisen
                Document withTag = await documentService.ModifyDocumentAsync(
                    new DocumentUpdate
                    {
                        DocumentId = original.Id,
                        TagIds = [addedTag.Id],
                    });

                // then
                withTag.Tags.Should().Contain(addedTag.Id);

                // when: leere Liste entfernt alle Tags
                Document withoutTags = await documentService.ModifyDocumentAsync(
                    new DocumentUpdate
                    {
                        DocumentId = original.Id,
                        TagIds = [],
                    });

                // then
                withoutTags.Tags.Should().BeEmpty();
            }
            finally
            {
                await IntegrationTestConfiguration.CleanUpAsync(
                    "Tags des Dokuments",
                    () => documentService.ModifyDocumentAsync(new DocumentUpdate
                    {
                        DocumentId = original.Id,
                        TagIds = originalTags,
                    }).AsTask());

                if (addedTag is not null)
                {
                    await IntegrationTestConfiguration.CleanUpAsync(
                        $"Tag {addedTag.Id}",
                        () => tagService.RemoveTagByIdAsync(addedTag.Id).AsTask());
                }
            }
        }

        [TestMethod]
        public async Task ShouldSetCustomFieldValueAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            IPaperlessBroker broker = IntegrationTestConfiguration.CreateBroker();
            ILoggingBroker loggingBroker = CreateLoggingBroker();

            var documentService = new DocumentService(broker, loggingBroker);
            var customFieldService = new CustomFieldService(broker, loggingBroker);

            Document original = await RequireAnyDocumentAsync(documentService);
            List<DocumentCustomFieldValue> originalValues = [.. original.CustomFields];

            CustomField? addedField = null;

            try
            {
                addedField = await customFieldService.AddCustomFieldAsync(new CustomField
                {
                    Name = $"zz-integrationstest-{Guid.NewGuid():N}"[..40],
                    DataType = CustomFieldDataType.Text,
                });

                // when
                Document withValue = await documentService.ModifyDocumentAsync(
                    new DocumentUpdate
                    {
                        DocumentId = original.Id,

                        CustomFields =
                        [
                            new DocumentCustomFieldValue
                            {
                                Field = addedField.Id,
                                Value = "Wert aus dem Integrationstest",
                            },
                        ],
                    });

                // then
                withValue.CustomFields.Should()
                    .Contain(value => value.Field == addedField.Id);

                DocumentCustomFieldValue actualValue = withValue.CustomFields
                    .Single(value => value.Field == addedField.Id);

                actualValue.Value!.ToString().Should().Be("Wert aus dem Integrationstest");
            }
            finally
            {
                await IntegrationTestConfiguration.CleanUpAsync(
                    "Feldwerte des Dokuments",
                    () => documentService.ModifyDocumentAsync(new DocumentUpdate
                    {
                        DocumentId = original.Id,
                        CustomFields = originalValues,
                    }).AsTask());

                if (addedField is not null)
                {
                    await IntegrationTestConfiguration.CleanUpAsync(
                        $"Feld {addedField.Id}",
                        () => customFieldService
                            .RemoveCustomFieldByIdAsync(addedField.Id).AsTask());
                }
            }
        }
    }
}
