using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string TagsRelativeUrl = "api/tags/";

        public async ValueTask<Tag> PostTagAsync(Tag tag) =>
            await PostAsync(TagsRelativeUrl, tag);

        public async ValueTask<Tag> GetTagByIdAsync(int tagId) =>
            await GetAsync<Tag>($"{TagsRelativeUrl}{tagId}/");

        public async ValueTask<TagPage> GetTagPageAsync(int page, int pageSize) =>
            await GetAsync<TagPage>($"{TagsRelativeUrl}?page={page}&page_size={pageSize}");

        public async ValueTask<Tag> PutTagAsync(Tag tag) =>
            await PutAsync($"{TagsRelativeUrl}{tag.Id}/", tag);

        // Paperless antwortet auf DELETE mit 204 ohne Inhalt, deshalb gibt der Broker
        // nichts zurueck statt eine leere Antwort zu deserialisieren.
        public async ValueTask DeleteTagByIdAsync(int tagId) =>
            await DeleteAsync($"{TagsRelativeUrl}{tagId}/");
    }
}
