using Paperless.Mcp.Models.Processings.CustomFields;

namespace Paperless.Mcp.Services.Processings.CustomFields
{
    public interface ICustomFieldProcessingService
    {
        // confirm entscheidet, ob wirklich geloescht wird. false ist der Default und
        // berichtet nur, was geschehen wuerde.
        ValueTask<CustomFieldRemovalPlan> RemoveCustomFieldByIdAsync(int customFieldId, bool confirm);
    }
}
