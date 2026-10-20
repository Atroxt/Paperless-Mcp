using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Statistics;
using Paperless.Mcp.Models.Foundations.Statistics.Exceptions;
using Paperless.Mcp.Services.Foundations.Statistics;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    [McpServerToolType]
    public class StatisticTools
    {
        private readonly IStatisticService statisticService;

        public StatisticTools(IStatisticService statisticService) =>
            this.statisticService = statisticService;

        [McpServerTool(Name = "paperless_statistics")]
        [Description(
            "Gets an overview of the Paperless instance: total number of documents, "
            + "documents in the inbox, inbox tag ids, counts of tags, correspondents, "
            + "document types and storage paths, character count, file type distribution "
            + "and the current archive serial number. Fields an older Paperless version "
            + "does not report are null.")]
        public async ValueTask<Statistic> GetStatisticsAsync()
        {
            try
            {
                return await this.statisticService.RetrieveStatisticAsync();
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                StatisticDependencyValidationException =>
                    new McpException(exception.InnerException?.Message ?? exception.Message),

                _ => new McpException(
                    "The Paperless server could not be reached or failed to answer. "
                    + "Check the server and the connection settings."),
            };
    }
}
