using System.Text.Json.Serialization;

namespace Paperless.Mcp.Models.Foundations.CustomFields
{
    // Anders als die Matching-Enums der anderen Entities ueberträgt Paperless diesen Typ
    // als Zeichenkette, nicht als Zahl. Der Konverter haengt deshalb am Typ selbst und
    // nicht an den globalen Optionen des Brokers -- sonst wuerden auch
    // TagMatchingAlgorithm und Verwandte als Text serialisiert und die API bräche.
    //
    // Die Namen sind nicht abgeleitet, sondern einzeln zugeordnet: "documentlink" und
    // "longtext" sind in Paperless ein Wort und wuerden sonst zu "document_link" und
    // "long_text". Die gueltige Liste stammt aus OPTIONS /api/custom_fields/.
    [JsonConverter(typeof(JsonStringEnumConverter<CustomFieldDataType>))]
    public enum CustomFieldDataType
    {
        [JsonStringEnumMemberName("string")]
        Text,

        [JsonStringEnumMemberName("url")]
        Url,

        [JsonStringEnumMemberName("date")]
        Date,

        [JsonStringEnumMemberName("boolean")]
        Boolean,

        [JsonStringEnumMemberName("integer")]
        Integer,

        [JsonStringEnumMemberName("float")]
        Float,

        [JsonStringEnumMemberName("monetary")]
        Monetary,

        [JsonStringEnumMemberName("documentlink")]
        DocumentLink,

        [JsonStringEnumMemberName("select")]
        Select,

        [JsonStringEnumMemberName("longtext")]
        LongText,
    }
}
