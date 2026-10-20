using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Foundations.StoragePaths.Exceptions;

namespace Paperless.Mcp.Services.Foundations.StoragePaths
{
    public partial class StoragePathService
    {
        private const int NameMaximumLength = 128;

        // Reihenfolge ist Pflicht: erst strukturell, dann logisch.
        private static void ValidateStoragePathOnAdd(StoragePath storagePath)
        {
            ValidateStoragePathIsNotNull(storagePath);

            Validate(
                (Rule: IsInvalid(storagePath.Name), Parameter: nameof(StoragePath.Name)),
                (Rule: ExceedsMaximumLength(storagePath.Name), Parameter: nameof(StoragePath.Name)),
                // Paperless antwortet ohne Path mit 400, also hier schon abfangen
                (Rule: IsInvalid(storagePath.Path), Parameter: nameof(StoragePath.Path)),
                (Rule: IsUndefined(storagePath.MatchingAlgorithm), Parameter: nameof(StoragePath.MatchingAlgorithm)));
        }

        private static void ValidateStoragePathOnModify(StoragePath storagePath)
        {
            ValidateStoragePathIsNotNull(storagePath);

            Validate(
                (Rule: IsInvalid(storagePath.Id), Parameter: nameof(StoragePath.Id)),
                (Rule: IsInvalid(storagePath.Name), Parameter: nameof(StoragePath.Name)),
                (Rule: ExceedsMaximumLength(storagePath.Name), Parameter: nameof(StoragePath.Name)),
                // Paperless antwortet ohne Path mit 400, also hier schon abfangen
                (Rule: IsInvalid(storagePath.Path), Parameter: nameof(StoragePath.Path)),
                (Rule: IsUndefined(storagePath.MatchingAlgorithm), Parameter: nameof(StoragePath.MatchingAlgorithm)));
        }

        private static void ValidateStoragePathId(int storagePathId) =>
            Validate((Rule: IsInvalid(storagePathId), Parameter: nameof(StoragePath.Id)));

        private static void ValidatePageRequest(int page, int pageSize) =>
            Validate(
                (Rule: IsInvalid(page), Parameter: "Page"),
                (Rule: IsInvalid(pageSize), Parameter: "PageSize"));

        private static void ValidateStoragePathIsNotNull(StoragePath storagePath)
        {
            if (storagePath is null)
            {
                throw new NullStoragePathException(message: "StoragePath is null.");
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required",
        };

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static dynamic ExceedsMaximumLength(string text) => new
        {
            Condition = text?.Length > NameMaximumLength,
            Message = $"Text exceeds the maximum length of {NameMaximumLength} characters",
        };

        private static dynamic IsUndefined(StoragePathMatchingAlgorithm matchingAlgorithm) => new
        {
            Condition = Enum.IsDefined(matchingAlgorithm) is false,
            Message = "Value is not a defined matching algorithm",
        };

        // Continuous validation: alle verletzten Regeln sammeln, dann den Stromkreis
        // unterbrechen -- der Aufrufer sieht jeden Fehler auf einmal.
        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidStoragePathException =
                new InvalidStoragePathException(
                    message: "StoragePath is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidStoragePathException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidStoragePathException.ThrowIfContainsErrors();
        }
    }
}
