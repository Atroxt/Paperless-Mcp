using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;
using Paperless.Mcp.Models.Processings.StoragePaths;
using Paperless.Mcp.Models.Processings.StoragePaths.Exceptions;
using Paperless.Mcp.Services.Foundations.StoragePaths;
using Paperless.Mcp.Services.Processings.StoragePaths;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Reines Duplex-Mapping, genau eine Service-Abhaengigkeit.
    //
    // Wie bei Tags bewusst noch kein Loesch-Tool: destruktive Operationen verlangen
    // confirm=true mit Dry-Run als Default, und diese Pruefung gehoert in eine
    // Processing Service. RemoveStoragePathByIdAsync existiert und ist getestet.
    [McpServerToolType]
    public class StoragePathTools
    {
        private readonly IStoragePathService storagePathService;
        private readonly IStoragePathProcessingService storagePathProcessingService;

        public StoragePathTools(
            IStoragePathService storagePathService,
            IStoragePathProcessingService storagePathProcessingService)
        {
            this.storagePathService = storagePathService;
            this.storagePathProcessingService = storagePathProcessingService;
        }

        [McpServerTool(Name = "paperless_storage_paths_list")]
        [Description("Lists storage paths from Paperless-ngx one page at a time.")]
        public async ValueTask<StoragePathPage> ListStoragePathsAsync(
            [Description("Page number, starting at 1.")] int page = 1,
            [Description("Number of storage paths per page.")] int pageSize = 25)
        {
            try
            {
                return await this.storagePathService.RetrieveStoragePathPageAsync(page, pageSize);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_storage_paths_get")]
        [Description("Gets a single storage path from Paperless-ngx by its id.")]
        public async ValueTask<StoragePath> GetStoragePathAsync(
            [Description("Id of the storage path.")] int storagePathId)
        {
            try
            {
                return await this.storagePathService.RetrieveStoragePathByIdAsync(storagePathId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_storage_paths_create")]
        [Description("Creates a new storage path in Paperless-ngx.")]
        public async ValueTask<StoragePath> CreateStoragePathAsync(
            [Description("Name of the storage path, at most 128 characters.")] string name,
            [Description("Target path with placeholders, for example {created_year}/{correspondent}/{title}. Paperless rewrites it into its own notation.")] string path,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] StoragePathMatchingAlgorithm matchingAlgorithm = StoragePathMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true)
        {
            var storagePath = new StoragePath
            {
                Name = name,
                Path = path,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
            };

            try
            {
                return await this.storagePathService.AddStoragePathAsync(storagePath);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_storage_paths_update")]
        [Description("Replaces an existing storage path in Paperless-ngx. Every field is written, omitted fields are cleared.")]
        public async ValueTask<StoragePath> UpdateStoragePathAsync(
            [Description("Id of the storage path to replace.")] int storagePathId,
            [Description("Name of the storage path, at most 128 characters.")] string name,
            [Description("Target path with placeholders, for example {created_year}/{correspondent}/{title}. Paperless rewrites it into its own notation.")] string path,
            [Description("Pattern the matching algorithm applies to incoming documents.")] string? match = null,
            [Description("How the match pattern is applied.")] StoragePathMatchingAlgorithm matchingAlgorithm = StoragePathMatchingAlgorithm.None,
            [Description("Whether the match pattern ignores case.")] bool isInsensitive = true)
        {
            var storagePath = new StoragePath
            {
                Id = storagePathId,
                Name = name,
                Path = path,
                Match = match,
                MatchingAlgorithm = matchingAlgorithm,
                IsInsensitive = isInsensitive,
            };

            try
            {
                return await this.storagePathService.ModifyStoragePathAsync(storagePath);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_storage_paths_delete")]
        [Description(
            "Deletes a storage path. Runs as a dry run unless confirm is true: it then "
            + "reports what would be deleted and how many documents are affected, and "
            + "changes nothing. Call again with confirm set to true to actually delete. "
            + "Documents keep existing and fall back to the default location.")]
        public async ValueTask<StoragePathRemovalPlan> DeleteStoragePathAsync(
            [Description("Id of the storage path.")] int storagePathId,
            [Description("Set to true to actually delete. Leave false to see what would happen.")] bool confirm = false)
        {
            try
            {
                return await this.storagePathProcessingService.RemoveStoragePathByIdAsync(storagePathId, confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                StoragePathValidationException
                    or StoragePathDependencyValidationException
                    or StoragePathProcessingValidationException
                    or StoragePathProcessingDependencyValidationException =>
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
