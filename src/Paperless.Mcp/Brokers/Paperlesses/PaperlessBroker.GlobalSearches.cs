using Paperless.Mcp.Models.Foundations.GlobalSearches;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string GlobalSearchRelativeUrl = "api/search/";

        public async ValueTask<PaperlessGlobalSearch> GetGlobalSearchAsync(string query) =>
            await GetAsync<PaperlessGlobalSearch>(
                $"{GlobalSearchRelativeUrl}?query={Uri.EscapeDataString(query)}");
    }
}
