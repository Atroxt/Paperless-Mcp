namespace Paperless.Mcp.Models.Processings.Documents
{
    // Dreiwertig, und das ist der ganze Zweck: bei einer Zuordnung wie dem Korrespondenten
    // gibt es drei Absichten, nicht zwei -- unberuehrt lassen, auf etwas setzen, leeren.
    //
    // Die Unterscheidung steckt in der Kombination: ist die Eigenschaft, die diesen Typ
    // haelt, selbst null, bleibt die Zuordnung unberuehrt. Ist sie gesetzt, entscheidet
    // Clear zwischen Leeren und Setzen.
    //
    // DocumentUpdate (Einzeldokument, PATCH) kann das noch nicht und deutet null
    // durchweg als "nicht aendern".
    public class DocumentReferenceUpdate
    {
        public int? Id { get; set; }

        public bool Clear { get; set; }
    }
}
