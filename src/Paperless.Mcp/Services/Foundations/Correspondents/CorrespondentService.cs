using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Correspondents;

namespace Paperless.Mcp.Services.Foundations.Correspondents
{
    public partial class CorrespondentService : ICorrespondentService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public CorrespondentService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Correspondent> AddCorrespondentAsync(Correspondent correspondent) =>
            TryCatch(async () =>
            {
                ValidateCorrespondentOnAdd(correspondent);

                return await this.paperlessBroker.PostCorrespondentAsync(correspondent);
            });

        public ValueTask<Correspondent> RetrieveCorrespondentByIdAsync(int correspondentId) =>
            TryCatch(async () =>
            {
                ValidateCorrespondentId(correspondentId);

                return await this.paperlessBroker.GetCorrespondentByIdAsync(correspondentId);
            });

        public ValueTask<CorrespondentPage> RetrieveCorrespondentPageAsync(int page, int pageSize) =>
            TryCatch(async () =>
            {
                ValidatePageRequest(page, pageSize);

                return await this.paperlessBroker.GetCorrespondentPageAsync(page, pageSize);
            });

        public ValueTask<Correspondent> ModifyCorrespondentAsync(Correspondent correspondent) =>
            TryCatch(async () =>
            {
                ValidateCorrespondentOnModify(correspondent);

                return await this.paperlessBroker.PutCorrespondentAsync(correspondent);
            });

        public ValueTask RemoveCorrespondentByIdAsync(int correspondentId) =>
            TryCatch(async () =>
            {
                ValidateCorrespondentId(correspondentId);

                await this.paperlessBroker.DeleteCorrespondentByIdAsync(correspondentId);
            });
    }
}
