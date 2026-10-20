using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;
using Paperless.Mcp.Services.Foundations.CustomFields;

namespace Paperless.Mcp.Tests.Integrations.Services
{
    // Hier steckt mehr drin als bei den anderen Entities: data_type ueberträgt Paperless
    // als Zeichenkette statt als Zahl, und extra_data ist ein verschachteltes Objekt,
    // dessen Options-Ids der Server selbst vergibt. Beides laesst sich nur an einer
    // echten Instanz belegen.
    [TestClass]
    public class CustomFieldServiceTests
    {
        private static ICustomFieldService CreateCustomFieldService()
        {
            IPaperlessBroker paperlessBroker = IntegrationTestConfiguration.CreateBroker();

            ILoggingBroker loggingBroker =
                new LoggingBroker(new NullLogger<LoggingBroker>());

            return new CustomFieldService(paperlessBroker, loggingBroker);
        }

        private static string CreateTestCustomFieldName() =>
            $"zz-integrationstest-{Guid.NewGuid():N}"[..40];

        [TestMethod]
        public async Task ShouldWalkThroughCustomFieldLifecycleAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ICustomFieldService customFieldService = CreateCustomFieldService();

            var newCustomField = new CustomField
            {
                Name = CreateTestCustomFieldName(),
                DataType = CustomFieldDataType.Text,
            };

            CustomField? addedCustomField = null;

            try
            {
                // when
                addedCustomField =
                    await customFieldService.AddCustomFieldAsync(newCustomField);

                // then
                addedCustomField.Id.Should().BeGreaterThan(0);
                addedCustomField.Name.Should().Be(newCustomField.Name);

                // Belegt die Zeichenketten-Zuordnung: Text entspricht "string"
                addedCustomField.DataType.Should().Be(CustomFieldDataType.Text);
                addedCustomField.ExtraData.Should().BeNull();

                // when: einzeln lesen
                CustomField retrievedCustomField =
                    await customFieldService.RetrieveCustomFieldByIdAsync(addedCustomField.Id);

                // then
                retrievedCustomField.DataType.Should().Be(CustomFieldDataType.Text);
                retrievedCustomField.DocumentCount.Should().Be(0);

                // when: aendern
                retrievedCustomField.Name = CreateTestCustomFieldName();
                CustomField modifiedCustomField =
                    await customFieldService.ModifyCustomFieldAsync(retrievedCustomField);

                // then
                modifiedCustomField.Name.Should().Be(retrievedCustomField.Name);

                // when: seitenweise lesen
                CustomFieldPage customFieldPage =
                    await customFieldService.RetrieveCustomFieldPageAsync(page: 1, pageSize: 100);

                // then
                customFieldPage.Results.Should()
                    .Contain(customField => customField.Id == addedCustomField.Id);
            }
            finally
            {
                if (addedCustomField is not null)
                {
                    await customFieldService.RemoveCustomFieldByIdAsync(addedCustomField.Id);
                }
            }
        }

        [TestMethod]
        public async Task ShouldRoundTripSelectOptionsAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ICustomFieldService customFieldService = CreateCustomFieldService();

            var newCustomField = new CustomField
            {
                Name = CreateTestCustomFieldName(),
                DataType = CustomFieldDataType.Select,
                ExtraData = new CustomFieldExtraData
                {
                    SelectOptions =
                    [
                        new CustomFieldSelectOption { Label = "Offen" },
                        new CustomFieldSelectOption { Label = "Bezahlt" },
                    ],
                },
            };

            CustomField? addedCustomField = null;

            try
            {
                // when
                addedCustomField =
                    await customFieldService.AddCustomFieldAsync(newCustomField);

                // then
                addedCustomField.DataType.Should().Be(CustomFieldDataType.Select);
                addedCustomField.ExtraData.Should().NotBeNull();
                addedCustomField.ExtraData!.SelectOptions.Should().HaveCount(2);

                addedCustomField.ExtraData.SelectOptions!
                    .Select(option => option.Label)
                    .Should().BeEquivalentTo(["Offen", "Bezahlt"]);

                // Die Ids der Optionen vergibt Paperless selbst
                addedCustomField.ExtraData.SelectOptions
                    .Should().AllSatisfy(option =>
                        option.Id.Should().NotBeNullOrWhiteSpace());
            }
            finally
            {
                if (addedCustomField is not null)
                {
                    await customFieldService.RemoveCustomFieldByIdAsync(addedCustomField.Id);
                }
            }
        }

        [TestMethod]
        public async Task ShouldRoundTripDefaultCurrencyAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ICustomFieldService customFieldService = CreateCustomFieldService();

            var newCustomField = new CustomField
            {
                Name = CreateTestCustomFieldName(),
                DataType = CustomFieldDataType.Monetary,
                ExtraData = new CustomFieldExtraData { DefaultCurrency = "EUR" },
            };

            CustomField? addedCustomField = null;

            try
            {
                // when
                addedCustomField =
                    await customFieldService.AddCustomFieldAsync(newCustomField);

                // then
                addedCustomField.DataType.Should().Be(CustomFieldDataType.Monetary);
                addedCustomField.ExtraData!.DefaultCurrency.Should().Be("EUR");
                addedCustomField.ExtraData.SelectOptions.Should().BeNull();
            }
            finally
            {
                if (addedCustomField is not null)
                {
                    await customFieldService.RemoveCustomFieldByIdAsync(addedCustomField.Id);
                }
            }
        }

        [TestMethod]
        public async Task ShouldThrowDependencyValidationExceptionOnRetrieveIfCustomFieldDoesNotExistAsync()
        {
            // given
            IntegrationTestConfiguration.SkipIfNotConfigured();
            ICustomFieldService customFieldService = CreateCustomFieldService();
            const int unlikelyCustomFieldId = 999_999_999;

            // when
            Func<Task<CustomField>> retrieveCustomFieldTask = async () =>
                await customFieldService.RetrieveCustomFieldByIdAsync(unlikelyCustomFieldId);

            // then
            await retrieveCustomFieldTask.Should()
                .ThrowAsync<CustomFieldDependencyValidationException>();
        }
    }
}
