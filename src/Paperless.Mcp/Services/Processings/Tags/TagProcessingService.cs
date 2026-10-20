using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Processings.Tags;
using Paperless.Mcp.Services.Foundations.Tags;

namespace Paperless.Mcp.Services.Processings.Tags
{
    // Hoeherwertige Logik einer Entity: holt erst den Tag, um die Tragweite zu kennen,
    // und entfernt ihn nur bei ausdruecklicher Bestaetigung. Zwei Schritte auf einer
    // Foundation Service -- genau das, wofuer die Processing-Schicht da ist.
    public partial class TagProcessingService : ITagProcessingService
    {
        private readonly ITagService tagService;

        public TagProcessingService(ITagService tagService) =>
            this.tagService = tagService;

        public ValueTask<TagRemovalPlan> RemoveTagByIdAsync(int tagId, bool confirm) =>
            TryCatch(async () =>
            {
                ValidateTagIdOnRemove(tagId);

                // Immer zuerst lesen: der Bericht soll die Tragweite nennen, und zwar
                // auch dann, wenn anschliessend geloescht wird.
                Tag tag = await this.tagService.RetrieveTagByIdAsync(tagId);

                if (confirm is false)
                {
                    return CreateDryRunPlan(tag);
                }

                await this.tagService.RemoveTagByIdAsync(tagId);

                return CreateRemovedPlan(tag);
            });

        private static TagRemovalPlan CreateDryRunPlan(Tag tag) =>
            new()
            {
                TagId = tag.Id,
                Name = tag.Name,
                DocumentCount = tag.DocumentCount,
                Removed = false,

                Summary =
                    $"Dry run: tag '{tag.Name}' would be deleted. "
                    + $"{DescribeDocumentCount(tag.DocumentCount)} "
                    + "Call again with confirm set to true to actually delete it.",
            };

        private static TagRemovalPlan CreateRemovedPlan(Tag tag) =>
            new()
            {
                TagId = tag.Id,
                Name = tag.Name,
                DocumentCount = tag.DocumentCount,
                Removed = true,

                Summary =
                    $"Tag '{tag.Name}' was deleted. "
                    + $"{DescribeDocumentCount(tag.DocumentCount)}",
            };

        private static string DescribeDocumentCount(int documentCount) =>
            documentCount is 0
                ? "No document carries it."
                : $"{documentCount} document(s) carry it and lose the assignment; "
                  + "the documents themselves are kept.";
    }
}
