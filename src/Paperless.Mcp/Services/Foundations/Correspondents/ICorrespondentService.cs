using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Services.Foundations.Correspondents
{
    // Foundation Service: uebersetzt Technologiesprache in Geschaeftssprache
    // (Post -> Add, Get -> Retrieve, Put -> Modify, Delete -> Remove) und ist die
    // letzte Abstraktionsschicht vor der eigentlichen Geschaeftslogik.
    public interface ICorrespondentService
    {
        ValueTask<Correspondent> AddCorrespondentAsync(Correspondent correspondent);
        ValueTask<Correspondent> RetrieveCorrespondentByIdAsync(int correspondentId);
        ValueTask<CorrespondentPage> RetrieveCorrespondentPageAsync(int page, int pageSize);
        ValueTask<Correspondent> ModifyCorrespondentAsync(Correspondent correspondent);
        ValueTask RemoveCorrespondentByIdAsync(int correspondentId);
    }
}
