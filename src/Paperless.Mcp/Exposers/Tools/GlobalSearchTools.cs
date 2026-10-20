using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.GlobalSearches;
using Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions;
using Paperless.Mcp.Services.Foundations.GlobalSearches;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    [McpServerToolType]
    public class GlobalSearchTools
    {
        private readonly IGlobalSearchService globalSearchService;

        public GlobalSearchTools(IGlobalSearchService globalSearchService) =>
            this.globalSearchService = globalSearchService;

        [McpServerTool(Name = "paperless_search")]
        [Description(
            "Searches documents, tags, correspondents, document types, storage paths and "
            + "custom fields at once and returns matching ids with their labels. Use this "
            + "to find out what exists under a name before calling a specific tool; for "
            + "document details use paperless_documents_search or paperless_documents_get.")]
        public async ValueTask<GlobalSearchResult> SearchAsync(
            [Description("What to look for, matched against names, titles and document text.")] string query)
        {
            try
            {
                return await this.globalSearchService.RetrieveGlobalSearchAsync(query);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                GlobalSearchValidationException or GlobalSearchDependencyValidationException =>
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
