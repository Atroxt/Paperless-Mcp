using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;
using Paperless.Mcp.Models.Processings.Tags;
using Paperless.Mcp.Models.Processings.Tags.Exceptions;
using Paperless.Mcp.Services.Foundations.Tags;
using Paperless.Mcp.Services.Processings.Tags;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Exposer: reines Duplex-Mapping zwischen MCP und der Geschaeftslogik. Genau eine
    // Service-Abhaengigkeit, keine Geschaeftslogik, kein Broker.
    //
    // Bewusst noch kein Loesch-Tool: destruktive Operationen verlangen laut Plan
    // confirm=true mit Dry-Run als Default, und diese Pruefung gehoert in eine
    // Processing Service, die es fuer Tags noch nicht gibt. RemoveTagByIdAsync der
    // Foundation Service existiert und ist getestet, wird hier aber nicht exponiert.
    [McpServerToolType]
    public class TagTools
    {
        private readonly ITagService tagService;
        private readonly ITagProcessingService tagProcessingService;

        public TagTools(
            ITagService tagService,
            ITagProcessingService tagProcessingService)
        {
            this.tagService = tagService;
            this.tagProcessingService = tagProcessingService;
        }

        [McpServerTool(Name = "paperless_tags_list")]
        [Description("Lists tags from Paperless-ngx one page at a time.")]
        public async ValueTask<TagPage> ListTagsAsync(
            [Description("Page number, starting at 1.")] int page = 1,
            [Description("Number of tags per page.")] int pageSize = 25)
        {
            try
            {
                return await this.tagService.RetrieveTagPageAsync(page, pageSize);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_tags_get")]
        [Description("Gets a single tag from Paperless-ngx by its id.")]
        public async ValueTask<Tag> GetTagAsync(
            [Description("Id of the tag.")] int tagId)
        {
            try
            {
                return await this.tagService.RetrieveTagByIdAsync(tagId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_tags_create")]
        [Description("Creates a new tag in Paperless-ngx.")]
        public async ValueTask<Tag> CreateTagAsync(
            [Description("Name of the tag, at most 128 characters.")] string name,
            [Description("Hex color such as #a1b2c3.")] string? color = null,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] TagMatchingAlgorithm matchingAlgorithm = TagMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true,
            [Description("Whether documents with this tag land in the inbox.")] bool isInboxTag = false)
        {
            var tag = new Tag
            {
                Name = name,
                Color = color,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
                IsInboxTag = isInboxTag,
            };

            try
            {
                return await this.tagService.AddTagAsync(tag);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_tags_update")]
        [Description("Replaces an existing tag in Paperless-ngx. Every field is written, omitted fields are cleared.")]
        public async ValueTask<Tag> UpdateTagAsync(
            [Description("Id of the tag to replace.")] int tagId,
            [Description("Name of the tag, at most 128 characters.")] string name,
            [Description("Hex color such as #a1b2c3.")] string? color = null,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] TagMatchingAlgorithm matchingAlgorithm = TagMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true,
            [Description("Whether documents with this tag land in the inbox.")] bool isInboxTag = false)
        {
            var tag = new Tag
            {
                Id = tagId,
                Name = name,
                Color = color,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
                IsInboxTag = isInboxTag,
            };

            try
            {
                return await this.tagService.ModifyTagAsync(tag);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        // Fehlerberichte treu und mit klarer Trennung weitergeben: was der Aufrufer
        // beheben kann, bekommt die Details; alles andere bleibt generisch.
        [McpServerTool(Name = "paperless_tags_delete")]
        [Description(
            "Deletes a tag. Runs as a dry run unless confirm is true: it then reports "
            + "what would be deleted and how many documents carry the tag, and changes "
            + "nothing. Call again with confirm set to true to actually delete. "
            + "Documents keep existing, they only lose the assignment.")]
        public async ValueTask<TagRemovalPlan> DeleteTagAsync(
            [Description("Id of the tag.")] int tagId,
            [Description("Set to true to actually delete. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.tagProcessingService.RemoveTagByIdAsync(tagId, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                TagValidationException
                    or TagDependencyValidationException
                    or TagProcessingValidationException
                    or TagProcessingDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The Paperless server could not be reached or failed to answer. "
                    + "Check the server and the connection settings."),
            };

        private static string DescribeInvalidInput(Xeption exception)
        {
            Exception? inner = exception.InnerException;
            string reason = inner?.Message ?? exception.Message;

            if (inner is null || inner.Data.Count is 0)
            {
                return reason;
            }

            // Xeption legt die Regelverstoesse als List<string> je Feld ab. Defensiv
            // gelesen: ein unerwarteter Typ darf die Fehlermeldung nicht sprengen.
            IEnumerable<string> details = inner.Data.Keys
                .Cast<object>()
                .Select(key => $"{key}: {Describe(inner.Data[key])}");

            return $"{reason} {string.Join("; ", details)}";
        }

        private static string Describe(object? value) =>
            value switch
            {
                IEnumerable<string> messages => string.Join(", ", messages),
                null => string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
    }
}
