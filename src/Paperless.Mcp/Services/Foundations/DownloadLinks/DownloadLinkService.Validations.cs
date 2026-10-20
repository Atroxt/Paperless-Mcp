using Paperless.Mcp.Models.Foundations.DownloadLinks;
using Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions;

namespace Paperless.Mcp.Services.Foundations.DownloadLinks
{
    public partial class DownloadLinkService
    {
        private static void ValidateOnIssue(int documentId, string token) =>
            Validate(
                (Rule: IsInvalid(documentId), Parameter: nameof(DownloadLink.DocumentId)),

                // Ohne Token waere der Link wertlos -- und ein Rueckfall auf das Token
                // des Betreibers kommt hier nicht in Frage.
                (Rule: IsInvalid(token), Parameter: nameof(DownloadLink.Token)));

        private static void ValidateId(string id) =>
            Validate((Rule: IsInvalid(id), Parameter: nameof(DownloadLink.Id)));

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

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDownloadLinkException =
                new InvalidDownloadLinkException(
                    message: "DownloadLink is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDownloadLinkException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDownloadLinkException.ThrowIfContainsErrors();
        }
    }
}
