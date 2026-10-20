using AwesomeAssertions;
using Moq;
using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;
using Paperless.Mcp.Services.Foundations.Documents;
using Paperless.Mcp.Services.Processings.Documents;

namespace Paperless.Mcp.Tests.Units.Services.Processings.Documents
{
    [TestClass]
    public partial class DocumentProcessingServiceTests
    {
        private const int MaxPageSize = 100;

        private readonly Mock<IDocumentService> documentServiceMock;
        private readonly IDocumentProcessingService documentProcessingService;

        public DocumentProcessingServiceTests()
        {
            this.documentServiceMock = new Mock<IDocumentService>();

            this.documentProcessingService = new DocumentProcessingService(
                documentService: this.documentServiceMock.Object,
                configurations: new DocumentProcessingConfigurations
                {
                    MaxPageSize = MaxPageSize,
                });
        }

        [TestMethod]
        public async Task ShouldCapPageSizeAtMaximumAsync()
        {
            // given
            var oversizedSearch = new DocumentSearch { Page = 1, PageSize = 5_000 };
            var storedPage = new DocumentPage();

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ReturnsAsync(storedPage);

            // when
            await this.documentProcessingService.SearchDocumentsAsync(oversizedSearch);

            // then
            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.Is<DocumentSearch>(search =>
                    search.PageSize == MaxPageSize)),
                        Times.Once);

            // Das Suchmodell des Aufrufers bleibt unveraendert
            oversizedSearch.PageSize.Should().Be(5_000);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldLeaveSmallPageSizeUntouchedAsync()
        {
            // given
            var search = new DocumentSearch { Page = 2, PageSize = 10, Query = "rechnung" };
            var storedPage = new DocumentPage();

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ReturnsAsync(storedPage);

            // when
            await this.documentProcessingService.SearchDocumentsAsync(search);

            // then
            // Alle uebrigen Kriterien muessen die Kopie unveraendert erreichen
            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.Is<DocumentSearch>(forwarded =>
                    forwarded.PageSize == 10
                    && forwarded.Page == 2
                    && forwarded.Query == "rechnung")),
                        Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldThrowValidationExceptionIfSearchIsNullAsync()
        {
            // given
            DocumentSearch? nullSearch = null;

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentProcessingService.SearchDocumentsAsync(nullSearch!);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        public async Task ShouldThrowValidationExceptionIfPageSizeIsInvalidAsync(int invalidPageSize)
        {
            // given
            var invalidSearch = new DocumentSearch { PageSize = invalidPageSize };

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentProcessingService.SearchDocumentsAsync(invalidSearch);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentProcessingValidationException>();

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapFoundationValidationToDependencyValidationAsync()
        {
            // given
            var search = new DocumentSearch { PageSize = 10 };

            var documentValidationException =
                new DocumentValidationException(
                    message: "Document validation error occurred, fix the errors and try again.",
                    innerException: new InvalidDocumentException(
                        message: "Document is invalid, fix the errors and try again."));

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(documentValidationException);

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentProcessingService.SearchDocumentsAsync(search);

            // then
            // Die Processing-Schicht reicht nicht durch, sie kategorisiert neu
            await searchTask.Should()
                .ThrowAsync<DocumentProcessingDependencyValidationException>();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapFoundationDependencyToProcessingDependencyAsync()
        {
            // given
            var search = new DocumentSearch { PageSize = 10 };

            var documentDependencyException =
                new DocumentDependencyException(
                    message: "Document dependency error occurred, contact support.",
                    innerException: new FailedDocumentDependencyException(
                        message: "Failed document dependency error occurred, contact support.",
                        innerException: new Exception()));

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(documentDependencyException);

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentProcessingService.SearchDocumentsAsync(search);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentProcessingDependencyException>();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }

        [TestMethod]
        public async Task ShouldMapUnexpectedErrorToProcessingServiceExceptionAsync()
        {
            // given
            var search = new DocumentSearch { PageSize = 10 };

            this.documentServiceMock.Setup(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()))
                    .ThrowsAsync(new Exception());

            // when
            Func<Task<DocumentPage>> searchTask = async () =>
                await this.documentProcessingService.SearchDocumentsAsync(search);

            // then
            await searchTask.Should()
                .ThrowAsync<DocumentProcessingServiceException>();

            this.documentServiceMock.Verify(service =>
                service.RetrieveDocumentPageAsync(It.IsAny<DocumentSearch>()),
                    Times.Once);

            this.documentServiceMock.VerifyNoOtherCalls();
        }
    }
}
