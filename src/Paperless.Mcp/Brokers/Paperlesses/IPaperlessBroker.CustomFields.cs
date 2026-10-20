using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<CustomField> PostCustomFieldAsync(CustomField customField);
        ValueTask<CustomField> GetCustomFieldByIdAsync(int customFieldId);
        ValueTask<CustomFieldPage> GetCustomFieldPageAsync(int page, int pageSize);
        ValueTask<CustomField> PutCustomFieldAsync(CustomField customField);
        ValueTask DeleteCustomFieldByIdAsync(int customFieldId);
    }
}
