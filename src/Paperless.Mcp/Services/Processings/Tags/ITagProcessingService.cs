using Paperless.Mcp.Models.Processings.Tags;

namespace Paperless.Mcp.Services.Processings.Tags
{
    public interface ITagProcessingService
    {
        // confirm entscheidet, ob wirklich geloescht wird. false ist der Default und
        // berichtet nur, was geschehen wuerde.
        ValueTask<TagRemovalPlan> RemoveTagByIdAsync(int tagId, bool confirm);
    }
}
