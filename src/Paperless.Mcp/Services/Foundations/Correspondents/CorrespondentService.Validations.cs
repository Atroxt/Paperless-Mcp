using Paperless.Mcp.Models.Foundations.Correspondents;
using Paperless.Mcp.Models.Foundations.Correspondents.Exceptions;

namespace Paperless.Mcp.Services.Foundations.Correspondents
{
    public partial class CorrespondentService
    {
        private const int NameMaximumLength = 128;

        // Reihenfolge ist Pflicht: erst strukturell, dann logisch.
        private static void ValidateCorrespondentOnAdd(Correspondent correspondent)
        {
            ValidateCorrespondentIsNotNull(correspondent);

            Validate(
                (Rule: IsInvalid(correspondent.Name), Parameter: nameof(Correspondent.Name)),
                (Rule: ExceedsMaximumLength(correspondent.Name), Parameter: nameof(Correspondent.Name)),
                (Rule: IsUndefined(correspondent.MatchingAlgorithm), Parameter: nameof(Correspondent.MatchingAlgorithm)));
        }

        private static void ValidateCorrespondentOnModify(Correspondent correspondent)
        {
            ValidateCorrespondentIsNotNull(correspondent);

            Validate(
                (Rule: IsInvalid(correspondent.Id), Parameter: nameof(Correspondent.Id)),
                (Rule: IsInvalid(correspondent.Name), Parameter: nameof(Correspondent.Name)),
                (Rule: ExceedsMaximumLength(correspondent.Name), Parameter: nameof(Correspondent.Name)),
                (Rule: IsUndefined(correspondent.MatchingAlgorithm), Parameter: nameof(Correspondent.MatchingAlgorithm)));
        }

        private static void ValidateCorrespondentId(int correspondentId) =>
            Validate((Rule: IsInvalid(correspondentId), Parameter: nameof(Correspondent.Id)));

        private static void ValidatePageRequest(int page, int pageSize) =>
            Validate(
                (Rule: IsInvalid(page), Parameter: "Page"),
                (Rule: IsInvalid(pageSize), Parameter: "PageSize"));

        private static void ValidateCorrespondentIsNotNull(Correspondent correspondent)
        {
            if (correspondent is null)
            {
                throw new NullCorrespondentException(message: "Correspondent is null.");
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

        private static dynamic IsUndefined(CorrespondentMatchingAlgorithm matchingAlgorithm) => new
        {
            Condition = Enum.IsDefined(matchingAlgorithm) is false,
            Message = "Value is not a defined matching algorithm",
        };

        // Continuous validation: alle verletzten Regeln sammeln, dann den Stromkreis
        // unterbrechen -- der Aufrufer sieht jeden Fehler auf einmal.
        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidCorrespondentException =
                new InvalidCorrespondentException(
                    message: "Correspondent is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidCorrespondentException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidCorrespondentException.ThrowIfContainsErrors();
        }
    }
}
