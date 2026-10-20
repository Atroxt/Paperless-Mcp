namespace Paperless.Mcp.Models.Foundations.CustomFields
{
    // Secondary Model: existiert nur als Bestandteil eines CustomField.
    //
    // Bewusst typisiert statt als freies JSON-Objekt. Paperless legt hier zwar beliebige
    // Zusatzdaten ab, aber nur diese beiden Formen sind heute im Einsatz, und ein
    // typisiertes Modell sagt dem Aufrufer, was er setzen darf.
    public class CustomFieldExtraData
    {
        // Pflicht bei DataType.Select: Paperless weist ein Auswahlfeld ohne Optionen ab.
        public List<CustomFieldSelectOption>? SelectOptions { get; set; }

        // Nur bei DataType.Monetary, etwa "EUR".
        public string? DefaultCurrency { get; set; }
    }
}
