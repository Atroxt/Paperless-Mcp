using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<Correspondent> PostCorrespondentAsync(Correspondent correspondent);
        ValueTask<Correspondent> GetCorrespondentByIdAsync(int correspondentId);
        ValueTask<CorrespondentPage> GetCorrespondentPageAsync(int page, int pageSize);
        ValueTask<Correspondent> PutCorrespondentAsync(Correspondent correspondent);
        ValueTask DeleteCorrespondentByIdAsync(int correspondentId);
    }
}
