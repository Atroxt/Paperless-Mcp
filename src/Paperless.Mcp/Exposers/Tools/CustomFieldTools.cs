using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.CustomFields;
using Paperless.Mcp.Models.Foundations.CustomFields.Exceptions;
using Paperless.Mcp.Models.Processings.CustomFields;
using Paperless.Mcp.Models.Processings.CustomFields.Exceptions;
using Paperless.Mcp.Services.Foundations.CustomFields;
using Paperless.Mcp.Services.Processings.CustomFields;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Reines Duplex-Mapping, genau eine Service-Abhaengigkeit.
    //
    // Diese Tools verwalten die Felddefinitionen. Werte an Dokumenten zuzuweisen kommt
    // mit den Dokumenten selbst: paperless_documents_set_custom_fields.
    [McpServerToolType]
    public class CustomFieldTools
    {
        private readonly ICustomFieldService customFieldService;
        private readonly ICustomFieldProcessingService customFieldProcessingService;

        public CustomFieldTools(
            ICustomFieldService customFieldService,
            ICustomFieldProcessingService customFieldProcessingService)
        {
            this.customFieldService = customFieldService;
            this.customFieldProcessingService = customFieldProcessingService;
        }

        [McpServerTool(Name = "paperless_custom_fields_list")]
        [Description("Lists custom field definitions from Paperless-ngx one page at a time.")]
        public async ValueTask<CustomFieldPage> ListCustomFieldsAsync(
            [Description("Page number, starting at 1.")] int page = 1,
            [Description("Number of custom fields per page.")] int pageSize = 25)
        {
            try
            {
                return await this.customFieldService.RetrieveCustomFieldPageAsync(page, pageSize);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_custom_fields_get")]
        [Description("Gets a single custom field definition from Paperless-ngx by its id.")]
        public async ValueTask<CustomField> GetCustomFieldAsync(
            [Description("Id of the custom field.")] int customFieldId)
        {
            try
            {
                return await this.customFieldService.RetrieveCustomFieldByIdAsync(customFieldId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_custom_fields_create")]
        [Description("Creates a new custom field definition in Paperless-ngx.")]
        public async ValueTask<CustomField> CreateCustomFieldAsync(
            [Description("Name of the custom field, at most 128 characters.")] string name,
            [Description("Data type of the field, one of string, url, date, boolean, integer, float, monetary, documentlink, select, longtext. The value select requires selectOptions.")] CustomFieldDataType dataType,
            [Description("Labels of the choices, required when dataType is Select.")] string[]? selectOptions = null,
            [Description("Currency code such as EUR, only used when dataType is Monetary.")] string? defaultCurrency = null)
        {
            var customField = new CustomField
            {
                Name = name,
                DataType = dataType,
                ExtraData = BuildExtraData(selectOptions, defaultCurrency),
            };

            try
            {
                return await this.customFieldService.AddCustomFieldAsync(customField);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_custom_fields_update")]
        [Description("Replaces an existing custom field definition in Paperless-ngx. Every field is written, omitted fields are cleared.")]
        public async ValueTask<CustomField> UpdateCustomFieldAsync(
            [Description("Id of the custom field to replace.")] int customFieldId,
            [Description("Name of the custom field, at most 128 characters.")] string name,
            [Description("Data type of the field, one of string, url, date, boolean, integer, float, monetary, documentlink, select, longtext. The value select requires selectOptions.")] CustomFieldDataType dataType,
            [Description("Labels of the choices, required when dataType is Select.")] string[]? selectOptions = null,
            [Description("Currency code such as EUR, only used when dataType is Monetary.")] string? defaultCurrency = null)
        {
            var customField = new CustomField
            {
                Id = customFieldId,
                Name = name,
                DataType = dataType,
                ExtraData = BuildExtraData(selectOptions, defaultCurrency),
            };

            try
            {
                return await this.customFieldService.ModifyCustomFieldAsync(customField);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        // Der Aufrufer gibt nur Beschriftungen an; die Ids der Optionen vergibt Paperless.
        private static CustomFieldExtraData? BuildExtraData(
            string[]? selectOptions,
            string? defaultCurrency)
        {
            if (selectOptions is null or { Length: 0 }
                && string.IsNullOrWhiteSpace(defaultCurrency))
            {
                return null;
            }

            return new CustomFieldExtraData
            {
                SelectOptions = selectOptions is null or { Length: 0 }
                    ? null
                    : [.. selectOptions.Select(label => new CustomFieldSelectOption { Label = label })],

                DefaultCurrency = string.IsNullOrWhiteSpace(defaultCurrency)
                    ? null
                    : defaultCurrency,
            };
        }

        [McpServerTool(Name = "paperless_custom_fields_delete")]
        [Description(
            "Deletes a custom field. Runs as a dry run unless confirm is true: it then "
            + "reports what would be deleted and how many documents are affected, and "
            + "changes nothing. Call again with confirm set to true to actually delete. "
            + "Documents keep existing, but their values for this field are removed.")]
        public async ValueTask<CustomFieldRemovalPlan> DeleteCustomFieldAsync(
            [Description("Id of the custom field.")] int customFieldId,
            [Description("Set to true to actually delete. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.customFieldProcessingService.RemoveCustomFieldByIdAsync(customFieldId, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                CustomFieldValidationException
                    or CustomFieldDependencyValidationException
                    or CustomFieldProcessingValidationException
                    or CustomFieldProcessingDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The Paperless server could not be reached or failed to answer. "
                    + "Check the server and the connection settings."),
            };

        private static string DescribeInvalidInput(Xeption exception)
        {
            Exception? inner = exception.InnerException;
            string reason = inner?.Message ?? exception.Message;

            if (inner is null || inner.Data.Count is 0)
            {
                return reason;
            }

            IEnumerable<string> details = inner.Data.Keys
                .Cast<object>()
                .Select(key => $"{key}: {Describe(inner.Data[key])}");

            return $"{reason} {string.Join("; ", details)}";
        }

        private static string Describe(object? value) =>
            value switch
            {
                IEnumerable<string> messages => string.Join(", ", messages),
                null => string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
    }
}
