using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Processings.Tags.Exceptions;

namespace Paperless.Mcp.Services.Processings.Tags
{
    public partial class TagProcessingService
    {
        // Processing Services validieren nur, was sie selbst verwenden. Die Id prueft
        // zwar auch die Foundation Service, aber hier wird mit ihr vorher schon
        // gearbeitet.
        private static void ValidateTagIdOnRemove(int tagId) =>
            Validate((Rule: IsInvalid(tagId), Parameter: nameof(Tag.Id)));

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidTagProcessingException =
                new InvalidTagProcessingException(
                    message: "Tag processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidTagProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidTagProcessingException.ThrowIfContainsErrors();
        }
    }
}
