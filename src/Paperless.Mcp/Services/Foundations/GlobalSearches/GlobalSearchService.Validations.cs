using Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions;

namespace Paperless.Mcp.Services.Foundations.GlobalSearches
{
    public partial class GlobalSearchService
    {
        // Paperless antwortet ohne query mit 400 "Query required", also hier schon
        // abfangen.
        private static void ValidateQuery(string query) =>
            Validate((Rule: IsInvalid(query), Parameter: "Query"));

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidGlobalSearchException =
                new InvalidGlobalSearchException(
                    message: "GlobalSearch is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidGlobalSearchException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidGlobalSearchException.ThrowIfContainsErrors();
        }
    }
}
