using Paperless.Mcp.Models.Foundations.CustomFields;

namespace Paperless.Mcp.Services.Foundations.CustomFields
{
    // Foundation Service: uebersetzt Technologiesprache in Geschaeftssprache
    // (Post -> Add, Get -> Retrieve, Put -> Modify, Delete -> Remove) und ist die
    // letzte Abstraktionsschicht vor der eigentlichen Geschaeftslogik.
    public interface ICustomFieldService
    {
        ValueTask<CustomField> AddCustomFieldAsync(CustomField customField);
        ValueTask<CustomField> RetrieveCustomFieldByIdAsync(int customFieldId);
        ValueTask<CustomFieldPage> RetrieveCustomFieldPageAsync(int page, int pageSize);
        ValueTask<CustomField> ModifyCustomFieldAsync(CustomField customField);
        ValueTask RemoveCustomFieldByIdAsync(int customFieldId);
    }
}
