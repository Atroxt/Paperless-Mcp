using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string CustomFieldsRelativeUrl = "api/custom_fields/";

        public async ValueTask<CustomField> PostCustomFieldAsync(CustomField customField) =>
            await PostAsync(CustomFieldsRelativeUrl, customField);

        public async ValueTask<CustomField> GetCustomFieldByIdAsync(int customFieldId) =>
            await GetAsync<CustomField>($"{CustomFieldsRelativeUrl}{customFieldId}/");

        public async ValueTask<CustomFieldPage> GetCustomFieldPageAsync(int page, int pageSize) =>
            await GetAsync<CustomFieldPage>(
                $"{CustomFieldsRelativeUrl}?page={page}&page_size={pageSize}");

        public async ValueTask<CustomField> PutCustomFieldAsync(CustomField customField) =>
            await PutAsync($"{CustomFieldsRelativeUrl}{customField.Id}/", customField);

        public async ValueTask DeleteCustomFieldByIdAsync(int customFieldId) =>
            await DeleteAsync($"{CustomFieldsRelativeUrl}{customFieldId}/");
    }
}
