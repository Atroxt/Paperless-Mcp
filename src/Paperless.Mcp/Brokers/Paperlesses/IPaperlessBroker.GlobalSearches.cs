using Paperless.Mcp.Models.Foundations.GlobalSearches;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<PaperlessGlobalSearch> GetGlobalSearchAsync(string query);
    }
}
