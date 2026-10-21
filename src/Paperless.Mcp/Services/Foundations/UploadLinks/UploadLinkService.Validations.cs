using Paperless.Mcp.Models.Foundations.UploadLinks;
using Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions;

namespace Paperless.Mcp.Services.Foundations.UploadLinks
{
    public partial class UploadLinkService
    {
        private static void ValidateOnIssue(UploadLink uploadLink)
        {
            if (uploadLink is null)
            {
                throw new NullUploadLinkException(message: "UploadLink is null.");
            }

            Validate(
                (Rule: IsInvalid(uploadLink.FileName), Parameter: nameof(UploadLink.FileName)),

                // Ohne Token waere der Link wertlos -- und ein Rueckfall auf das Token
                // des Betreibers kommt hier nicht in Frage.
                (Rule: IsInvalid(uploadLink.Token), Parameter: nameof(UploadLink.Token)),
                (Rule: IsInvalid(uploadLink.CorrespondentId), Parameter: nameof(UploadLink.CorrespondentId)),
                (Rule: IsInvalid(uploadLink.DocumentTypeId), Parameter: nameof(UploadLink.DocumentTypeId)),
                (Rule: IsInvalid(uploadLink.StoragePathId), Parameter: nameof(UploadLink.StoragePathId)));
        }

        private static void ValidateId(string id) =>
            Validate((Rule: IsInvalid(id), Parameter: nameof(UploadLink.Id)));

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required",
        };

        // null ist gueltig und heisst "nicht zuordnen".
        private static dynamic IsInvalid(int? number) => new
        {
            Condition = number is not null && number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidUploadLinkException =
                new InvalidUploadLinkException(
                    message: "UploadLink is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidUploadLinkException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidUploadLinkException.ThrowIfContainsErrors();
        }
    }
}
