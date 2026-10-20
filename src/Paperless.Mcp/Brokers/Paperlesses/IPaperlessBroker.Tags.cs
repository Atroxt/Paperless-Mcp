using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<Tag> PostTagAsync(Tag tag);
        ValueTask<Tag> GetTagByIdAsync(int tagId);
        ValueTask<TagPage> GetTagPageAsync(int page, int pageSize);
        ValueTask<Tag> PutTagAsync(Tag tag);
        ValueTask DeleteTagByIdAsync(int tagId);
    }
}
