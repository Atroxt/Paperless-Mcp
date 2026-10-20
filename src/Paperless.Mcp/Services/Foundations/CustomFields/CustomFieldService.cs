using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Services.Foundations.CustomFields
{
    public partial class CustomFieldService : ICustomFieldService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public CustomFieldService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<CustomField> AddCustomFieldAsync(CustomField customField) =>
            TryCatch(async () =>
            {
                ValidateCustomFieldOnAdd(customField);

                return await this.paperlessBroker.PostCustomFieldAsync(customField);
            });

        public ValueTask<CustomField> RetrieveCustomFieldByIdAsync(int customFieldId) =>
            TryCatch(async () =>
            {
                ValidateCustomFieldId(customFieldId);

                return await this.paperlessBroker.GetCustomFieldByIdAsync(customFieldId);
            });

        public ValueTask<CustomFieldPage> RetrieveCustomFieldPageAsync(int page, int pageSize) =>
            TryCatch(async () =>
            {
                ValidatePageRequest(page, pageSize);

                return await this.paperlessBroker.GetCustomFieldPageAsync(page, pageSize);
            });

        public ValueTask<CustomField> ModifyCustomFieldAsync(CustomField customField) =>
            TryCatch(async () =>
            {
                ValidateCustomFieldOnModify(customField);

                return await this.paperlessBroker.PutCustomFieldAsync(customField);
            });

        public ValueTask RemoveCustomFieldByIdAsync(int customFieldId) =>
            TryCatch(async () =>
            {
                ValidateCustomFieldId(customFieldId);

                await this.paperlessBroker.DeleteCustomFieldByIdAsync(customFieldId);
            });
    }
}
