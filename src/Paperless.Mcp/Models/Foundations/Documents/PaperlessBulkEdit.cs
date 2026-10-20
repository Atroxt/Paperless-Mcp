namespace Paperless.Mcp.Models.Foundations.Documents
{
    // Drahtform von POST /api/documents/bulk_edit/.
    //
    // Reprocess ist nur hierueber erreichbar: der eigene Endpunkt
    // /api/documents/{id}/reprocess/ scheitert mit Token-Authentifizierung an der
    // CSRF-Pruefung von Django -- an der Instanz geprueft.
    //
    // Das Feld "all" der API wird bewusst NICHT abgebildet. Es wuerde einen Aufruf auf
    // den gesamten Bestand wirken lassen, und so etwas gehoert nicht in ein Werkzeug,
    // das ein Assistent auf Zuruf aufruft.
    public class PaperlessBulkEdit
    {
        public List<int> Documents { get; set; } = [];

        public string Method { get; set; } = string.Empty;

        // Der Wert darf null sein, und zwar tragend: {"correspondent": null} LEERT die
        // Zuordnung, waehrend ein weggelassenes Feld sie unberuehrt liesse. Dass die
        // Serialisierungsoptionen des Brokers (DefaultIgnoreCondition.WhenWritingNull)
        // Verzeichniswerte nicht wegwerfen, ist nachgemessen und nicht angenommen.
        public Dictionary<string, object?> Parameters { get; set; } = [];

        public const string ReprocessMethod = "reprocess";

        // An der Instanz ueber OPTIONS /api/documents/bulk_edit/ abgefragt. Die uebrigen
        // Methoden (merge, split, rotate, edit_pdf, delete_pages, remove_password,
        // set_permissions) schreiben Dateien oder Rechte um und sind bewusst nicht
        // abgebildet.
        public const string SetCorrespondentMethod = "set_correspondent";

        public const string SetDocumentTypeMethod = "set_document_type";

        public const string SetStoragePathMethod = "set_storage_path";

        public const string ModifyTagsMethod = "modify_tags";

        public const string DeleteMethod = "delete";

        // Parameternamen, wie die API sie verlangt -- an der Instanz abgefragt, indem
        // jede Methode mit leeren Parametern gerufen wurde: die Fehlermeldung nennt das
        // naechste fehlende Feld.
        public const string CorrespondentParameter = "correspondent";

        public const string DocumentTypeParameter = "document_type";

        public const string StoragePathParameter = "storage_path";

        public const string AddTagsParameter = "add_tags";

        public const string RemoveTagsParameter = "remove_tags";
    }
}
