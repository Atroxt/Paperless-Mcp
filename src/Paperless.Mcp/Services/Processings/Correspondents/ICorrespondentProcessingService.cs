using Paperless.Mcp.Models.Processings.Correspondents;

namespace Paperless.Mcp.Services.Processings.Correspondents
{
    public interface ICorrespondentProcessingService
    {
        // confirm entscheidet, ob wirklich geloescht wird. false ist der Default und
        // berichtet nur, was geschehen wuerde.
        ValueTask<CorrespondentRemovalPlan> RemoveCorrespondentByIdAsync(int correspondentId, bool confirm);
    }
}
