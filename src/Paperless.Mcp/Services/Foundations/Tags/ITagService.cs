using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Services.Foundations.Tags
{
    // Foundation Service: uebersetzt Technologiesprache in Geschaeftssprache
    // (Post -> Add, Get -> Retrieve, Put -> Modify, Delete -> Remove) und ist die
    // letzte Abstraktionsschicht vor der eigentlichen Geschaeftslogik.
    public interface ITagService
    {
        ValueTask<Tag> AddTagAsync(Tag tag);
        ValueTask<Tag> RetrieveTagByIdAsync(int tagId);
        ValueTask<TagPage> RetrieveTagPageAsync(int page, int pageSize);
        ValueTask<Tag> ModifyTagAsync(Tag tag);
        ValueTask RemoveTagByIdAsync(int tagId);
    }
}
