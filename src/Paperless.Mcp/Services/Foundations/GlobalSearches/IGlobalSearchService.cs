using Paperless.Mcp.Models.Foundations.GlobalSearches;

namespace Paperless.Mcp.Services.Foundations.GlobalSearches
{
    public interface IGlobalSearchService
    {
        ValueTask<GlobalSearchResult> RetrieveGlobalSearchAsync(string query);
    }
}
