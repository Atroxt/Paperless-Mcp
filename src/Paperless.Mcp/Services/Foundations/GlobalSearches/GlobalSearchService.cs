using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.GlobalSearches;

namespace Paperless.Mcp.Services.Foundations.GlobalSearches
{
    public partial class GlobalSearchService : IGlobalSearchService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public GlobalSearchService(
            IPaperlessBroker paperlessBroker,
            ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<GlobalSearchResult> RetrieveGlobalSearchAsync(string query) =>
            TryCatch(async () =>
            {
                ValidateQuery(query);

                PaperlessGlobalSearch paperlessGlobalSearch =
                    await this.paperlessBroker.GetGlobalSearchAsync(query);

                return MapToGlobalSearchResult(paperlessGlobalSearch);
            });

        // Abbildung der Technologiesprache auf die Geschaeftssprache: aus title und name
        // wird einheitlich Label, aus zwoelf Sammlungen werden die sechs fachlichen.
        private static GlobalSearchResult MapToGlobalSearchResult(
            PaperlessGlobalSearch paperlessGlobalSearch) =>
            new()
            {
                Total = paperlessGlobalSearch.Total,

                Documents = [.. paperlessGlobalSearch.Documents.Select(hit =>
                    new GlobalSearchHit { Id = hit.Id, Label = hit.Title })],

                Tags = MapNamed(paperlessGlobalSearch.Tags),
                Correspondents = MapNamed(paperlessGlobalSearch.Correspondents),
                DocumentTypes = MapNamed(paperlessGlobalSearch.DocumentTypes),
                StoragePaths = MapNamed(paperlessGlobalSearch.StoragePaths),
                CustomFields = MapNamed(paperlessGlobalSearch.CustomFields),
            };

        private static List<GlobalSearchHit> MapNamed(
            List<PaperlessGlobalSearch.NamedHit> namedHits) =>
            [.. namedHits.Select(hit =>
                new GlobalSearchHit { Id = hit.Id, Label = hit.Name })];
    }
}
