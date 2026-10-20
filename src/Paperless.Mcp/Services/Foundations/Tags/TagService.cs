using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Tags;

namespace Paperless.Mcp.Services.Foundations.Tags
{
    public partial class TagService : ITagService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public TagService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Tag> AddTagAsync(Tag tag) =>
            TryCatch(async () =>
            {
                ValidateTagOnAdd(tag);

                return await this.paperlessBroker.PostTagAsync(tag);
            });

        public ValueTask<Tag> RetrieveTagByIdAsync(int tagId) =>
            TryCatch(async () =>
            {
                ValidateTagId(tagId);

                return await this.paperlessBroker.GetTagByIdAsync(tagId);
            });

        public ValueTask<TagPage> RetrieveTagPageAsync(int page, int pageSize) =>
            TryCatch(async () =>
            {
                ValidatePageRequest(page, pageSize);

                return await this.paperlessBroker.GetTagPageAsync(page, pageSize);
            });

        public ValueTask<Tag> ModifyTagAsync(Tag tag) =>
            TryCatch(async () =>
            {
                ValidateTagOnModify(tag);

                return await this.paperlessBroker.PutTagAsync(tag);
            });

        public ValueTask RemoveTagByIdAsync(int tagId) =>
            TryCatch(async () =>
            {
                ValidateTagId(tagId);

                await this.paperlessBroker.DeleteTagByIdAsync(tagId);
            });
    }
}
