using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string CorrespondentsRelativeUrl = "api/correspondents/";

        public async ValueTask<Correspondent> PostCorrespondentAsync(Correspondent correspondent) =>
            await PostAsync(CorrespondentsRelativeUrl, correspondent);

        public async ValueTask<Correspondent> GetCorrespondentByIdAsync(int correspondentId) =>
            await GetAsync<Correspondent>($"{CorrespondentsRelativeUrl}{correspondentId}/");

        public async ValueTask<CorrespondentPage> GetCorrespondentPageAsync(int page, int pageSize) =>
            await GetAsync<CorrespondentPage>(
                $"{CorrespondentsRelativeUrl}?page={page}&page_size={pageSize}");

        public async ValueTask<Correspondent> PutCorrespondentAsync(Correspondent correspondent) =>
            await PutAsync($"{CorrespondentsRelativeUrl}{correspondent.Id}/", correspondent);

        public async ValueTask DeleteCorrespondentByIdAsync(int correspondentId) =>
            await DeleteAsync($"{CorrespondentsRelativeUrl}{correspondentId}/");
    }
}
