using Paperless.Mcp.Models.Foundations.StoragePaths;
using Paperless.Mcp.Models.Processings.StoragePaths.Exceptions;

namespace Paperless.Mcp.Services.Processings.StoragePaths
{
    public partial class StoragePathProcessingService
    {
        // Processing Services validieren nur, was sie selbst verwenden. Die Id prueft
        // zwar auch die Foundation Service, aber hier wird mit ihr vorher schon
        // gearbeitet.
        private static void ValidateStoragePathIdOnRemove(int storagePathId) =>
            Validate((Rule: IsInvalid(storagePathId), Parameter: nameof(StoragePath.Id)));

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidStoragePathProcessingException =
                new InvalidStoragePathProcessingException(
                    message: "StoragePath processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidStoragePathProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidStoragePathProcessingException.ThrowIfContainsErrors();
        }
    }
}
