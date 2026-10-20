using System.Text.RegularExpressions;
using Paperless.Mcp.Models.Foundations.Tags;
using Paperless.Mcp.Models.Foundations.Tags.Exceptions;

namespace Paperless.Mcp.Services.Foundations.Tags
{
    public partial class TagService
    {
        private const int NameMaximumLength = 128;

        // Paperless akzeptiert Hex-Farben in Kurz- und Langform.
        [GeneratedRegex("^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")]
        private static partial Regex HexColorPattern();

        // Reihenfolge ist Pflicht: erst strukturell, dann logisch.
        private static void ValidateTagOnAdd(Tag tag)
        {
            ValidateTagIsNotNull(tag);

            Validate(
                (Rule: IsInvalid(tag.Name), Parameter: nameof(Tag.Name)),
                (Rule: ExceedsMaximumLength(tag.Name), Parameter: nameof(Tag.Name)),
                (Rule: IsInvalidColor(tag.Color), Parameter: nameof(Tag.Color)),
                (Rule: IsUndefined(tag.MatchingAlgorithm), Parameter: nameof(Tag.MatchingAlgorithm)));
        }

        private static void ValidateTagOnModify(Tag tag)
        {
            ValidateTagIsNotNull(tag);

            Validate(
                (Rule: IsInvalid(tag.Id), Parameter: nameof(Tag.Id)),
                (Rule: IsInvalid(tag.Name), Parameter: nameof(Tag.Name)),
                (Rule: ExceedsMaximumLength(tag.Name), Parameter: nameof(Tag.Name)),
                (Rule: IsInvalidColor(tag.Color), Parameter: nameof(Tag.Color)),
                (Rule: IsUndefined(tag.MatchingAlgorithm), Parameter: nameof(Tag.MatchingAlgorithm)));
        }

        private static void ValidateTagId(int tagId) =>
            Validate((Rule: IsInvalid(tagId), Parameter: nameof(Tag.Id)));

        private static void ValidatePageRequest(int page, int pageSize) =>
            Validate(
                (Rule: IsInvalid(page), Parameter: "Page"),
                (Rule: IsInvalid(pageSize), Parameter: "PageSize"));

        private static void ValidateTagIsNotNull(Tag tag)
        {
            if (tag is null)
            {
                throw new NullTagException(message: "Tag is null.");
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

        private static dynamic IsInvalidColor(string? color) => new
        {
            Condition = color is not null && !HexColorPattern().IsMatch(color),
            Message = "Color must be a hex value such as #a1b2c3",
        };

        private static dynamic IsUndefined(TagMatchingAlgorithm matchingAlgorithm) => new
        {
            Condition = !Enum.IsDefined(matchingAlgorithm),
            Message = "Value is not a defined matching algorithm",
        };

        // Continuous validation: alle verletzten Regeln sammeln, dann den Stromkreis
        // unterbrechen -- der Aufrufer sieht jeden Fehler auf einmal.
        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidTagException =
                new InvalidTagException(
                    message: "Tag is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidTagException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidTagException.ThrowIfContainsErrors();
        }
    }
}
