using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;
using Paperless.Mcp.Models.Processings.Correspondents;
using Paperless.Mcp.Models.Processings.Correspondents.Exceptions;
using Paperless.Mcp.Services.Foundations.Correspondents;
using Paperless.Mcp.Services.Processings.Correspondents;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Reines Duplex-Mapping, genau eine Service-Abhaengigkeit.
    [McpServerToolType]
    public class CorrespondentTools
    {
        private readonly ICorrespondentService correspondentService;
        private readonly ICorrespondentProcessingService correspondentProcessingService;

        public CorrespondentTools(
            ICorrespondentService correspondentService,
            ICorrespondentProcessingService correspondentProcessingService)
        {
            this.correspondentService = correspondentService;
            this.correspondentProcessingService = correspondentProcessingService;
        }

        [McpServerTool(Name = "paperless_correspondents_list")]
        [Description("Lists correspondents from Paperless-ngx one page at a time.")]
        public async ValueTask<CorrespondentPage> ListCorrespondentsAsync(
            [Description("Page number, starting at 1.")] int page = 1,
            [Description("Number of correspondents per page.")] int pageSize = 25)
        {
            try
            {
                return await this.correspondentService.RetrieveCorrespondentPageAsync(page, pageSize);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_correspondents_get")]
        [Description("Gets a single correspondent from Paperless-ngx by its id.")]
        public async ValueTask<Correspondent> GetCorrespondentAsync(
            [Description("Id of the correspondent.")] int correspondentId)
        {
            try
            {
                return await this.correspondentService.RetrieveCorrespondentByIdAsync(correspondentId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_correspondents_create")]
        [Description("Creates a new correspondent in Paperless-ngx.")]
        public async ValueTask<Correspondent> CreateCorrespondentAsync(
            [Description("Name of the correspondent, at most 128 characters.")] string name,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] CorrespondentMatchingAlgorithm matchingAlgorithm = CorrespondentMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true)
        {
            var correspondent = new Correspondent
            {
                Name = name,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
            };

            try
            {
                return await this.correspondentService.AddCorrespondentAsync(correspondent);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_correspondents_update")]
        [Description("Replaces an existing correspondent in Paperless-ngx. Every field is written, omitted fields are cleared.")]
        public async ValueTask<Correspondent> UpdateCorrespondentAsync(
            [Description("Id of the correspondent to replace.")] int correspondentId,
            [Description("Name of the correspondent, at most 128 characters.")] string name,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] CorrespondentMatchingAlgorithm matchingAlgorithm = CorrespondentMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true)
        {
            var correspondent = new Correspondent
            {
                Id = correspondentId,
                Name = name,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
            };

            try
            {
                return await this.correspondentService.ModifyCorrespondentAsync(correspondent);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_correspondents_delete")]
        [Description(
            "Deletes a correspondent. Runs as a dry run unless confirm is true: it then "
            + "reports what would be deleted and how many documents are affected, and "
            + "changes nothing. Call again with confirm set to true to actually delete. "
            + "Documents keep existing, they only lose the assignment.")]
        public async ValueTask<CorrespondentRemovalPlan> DeleteCorrespondentAsync(
            [Description("Id of the correspondent.")] int correspondentId,
            [Description("Set to true to actually delete. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.correspondentProcessingService.RemoveCorrespondentByIdAsync(correspondentId, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                CorrespondentValidationException
                    or CorrespondentDependencyValidationException
                    or CorrespondentProcessingValidationException
                    or CorrespondentProcessingDependencyValidationException =>
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
