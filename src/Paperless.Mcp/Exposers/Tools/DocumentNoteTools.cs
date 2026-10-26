using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Foundations.Notes.Exceptions;
using Paperless.Mcp.Models.Processings.Notes;
using Paperless.Mcp.Models.Processings.Notes.Exceptions;
using Paperless.Mcp.Services.Foundations.Notes;
using Paperless.Mcp.Services.Processings.Notes;
using Xeptions;

namespace Paperless.Mcp.Exposers.Tools
{
    // Exposer: reines Duplex-Mapping zwischen MCP und der Geschaeftslogik. Keine
    // Geschaeftslogik, kein Broker.
    //
    // Kein Aendern-Tool: die Paperless-API kennt kein Update auf Notizen. Wer eine
    // Notiz berichtigen will, legt eine neue an und loescht die alte.
    [McpServerToolType]
    public class DocumentNoteTools
    {
        private readonly IDocumentNoteService documentNoteService;
        private readonly IDocumentNoteProcessingService documentNoteProcessingService;

        public DocumentNoteTools(
            IDocumentNoteService documentNoteService,
            IDocumentNoteProcessingService documentNoteProcessingService)
        {
            this.documentNoteService = documentNoteService;
            this.documentNoteProcessingService = documentNoteProcessingService;
        }

        [McpServerTool(Name = "paperless_document_notes_list")]
        [Description("Lists all notes attached to a document in Paperless-ngx.")]
        public async ValueTask<List<DocumentNote>> ListDocumentNotesAsync(
            [Description("Id of the document.")] int documentId)
        {
            try
            {
                return await this.documentNoteService.RetrieveDocumentNotesAsync(documentId);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        [McpServerTool(Name = "paperless_document_notes_create")]
        [Description(
            "Adds a note to a document in Paperless-ngx and returns all notes of that "
            + "document afterwards.")]
        public async ValueTask<List<DocumentNote>> CreateDocumentNoteAsync(
            [Description("Id of the document.")] int documentId,
            [Description("Text of the note, at most 10000 characters.")] string note)
        {
            try
            {
                return await this.documentNoteService.AddDocumentNoteAsync(documentId, note);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        // Trockenlauf als Default: ohne confirm wird nichts geloescht, sondern
        // berichtet, welche Notiz es traefe. Eine Notiz laesst sich nicht
        // wiederherstellen, und ihre Id allein sagt nichts ueber ihren Inhalt.
        [McpServerTool(Name = "paperless_document_notes_delete")]
        [Description(
            "Deletes a single note from a document in Paperless-ngx. Runs as a dry run "
            + "unless confirm is true: it then only reports which note would be deleted. "
            + "The document itself is never affected. There is no way to edit a note: "
            + "delete it and create a new one instead.")]
        public async ValueTask<DocumentNoteRemovalPlan> DeleteDocumentNoteAsync(
            [Description("Id of the document.")] int documentId,
            [Description("Id of the note to delete.")] int noteId,
            [Description("Set to true to actually delete the note. Defaults to a dry run.")]
            bool confirm = false)
        {
            try
            {
                return await this.documentNoteProcessingService.RemoveDocumentNoteAsync(
                    documentId,
                    noteId,
                    confirm);
            }
            catch (Xeption exception)
            {
                throw ToolError(exception);
            }
        }

        // Fehlerberichte treu und mit klarer Trennung weitergeben: was der Aufrufer
        // beheben kann, bekommt die Details; alles andere bleibt generisch.
        private static McpException ToolError(Xeption exception) =>
            exception switch
            {
                DocumentNoteValidationException
                    or DocumentNoteDependencyValidationException
                    or DocumentNoteProcessingValidationException
                    or DocumentNoteProcessingDependencyValidationException =>
                    new McpException(DescribeInvalidInput(exception)),

                _ => new McpException(
                    "The Paperless server could not be reached or failed to answer. "
                    + "Check the server and the connection settings."),
            };

        private static string DescribeInvalidInput(Xeption exception)
        {
            Exception? inner = exception.InnerException;
            string reason = inner?.Message ?? exception.Message;

            if (inner is null || inner.Data.Count is 0)
            {
                return reason;
            }

            // Xeption legt die Regelverstoesse als List<string> je Feld ab. Defensiv
            // gelesen: ein unerwarteter Typ darf die Fehlermeldung nicht sprengen.
            IEnumerable<string> details = inner.Data.Keys
                .Cast<object>()
                .Select(key => $"{key}: {Describe(inner.Data[key])}");

            return $"{reason} {string.Join("; ", details)}";
        }

        private static string Describe(object? value) =>
            value switch
            {
                IEnumerable<string> messages => string.Join(", ", messages),
                null => string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
    }
}
