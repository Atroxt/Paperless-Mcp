using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Services.Foundations.DocumentTypes
{
    public partial class DocumentTypeService : IDocumentTypeService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public DocumentTypeService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<DocumentType> AddDocumentTypeAsync(DocumentType documentType) =>
            TryCatch(async () =>
            {
                ValidateDocumentTypeOnAdd(documentType);

                return await this.paperlessBroker.PostDocumentTypeAsync(documentType);
            });

        public ValueTask<DocumentType> RetrieveDocumentTypeByIdAsync(int documentTypeId) =>
            TryCatch(async () =>
            {
                ValidateDocumentTypeId(documentTypeId);

                return await this.paperlessBroker.GetDocumentTypeByIdAsync(documentTypeId);
            });

        public ValueTask<DocumentTypePage> RetrieveDocumentTypePageAsync(int page, int pageSize) =>
            TryCatch(async () =>
            {
                ValidatePageRequest(page, pageSize);

                return await this.paperlessBroker.GetDocumentTypePageAsync(page, pageSize);
            });

        public ValueTask<DocumentType> ModifyDocumentTypeAsync(DocumentType documentType) =>
            TryCatch(async () =>
            {
                ValidateDocumentTypeOnModify(documentType);

                return await this.paperlessBroker.PutDocumentTypeAsync(documentType);
            });

        public ValueTask RemoveDocumentTypeByIdAsync(int documentTypeId) =>
            TryCatch(async () =>
            {
                ValidateDocumentTypeId(documentTypeId);

                await this.paperlessBroker.DeleteDocumentTypeByIdAsync(documentTypeId);
            });
    }
}
