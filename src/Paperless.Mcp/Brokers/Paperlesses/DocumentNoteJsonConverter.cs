using System.Text.Json;
using System.Text.Json.Serialization;
using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    // Paperless liefert "user" je nach Version als Id oder als eingebettetes Objekt.
    // Diese Unebenheit ist Technologiesprache und gehoert damit in den Broker -- das
    // Modell bleibt flach und kennt nur die Id.
    //
    // Der Converter haengt am ganzen Typ statt an der einzelnen Eigenschaft, weil ein
    // Converter fuer int? sonst jedes andere int?-Feld der API mit betreffen wuerde.
    internal sealed class DocumentNoteJsonConverter : JsonConverter<DocumentNote>
    {
        public override DocumentNote Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected an object for a document note.");
            }

            var documentNote = new DocumentNote();

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    continue;
                }

                string propertyName = reader.GetString()!;
                reader.Read();

                switch (propertyName)
                {
                    case "id":
                        documentNote.Id = reader.GetInt32();
                        break;

                    case "note":
                        documentNote.Note = reader.GetString() ?? string.Empty;
                        break;

                    case "created":
                        documentNote.Created = reader.GetDateTimeOffset();
                        break;

                    case "user":
                        documentNote.User = ReadUser(ref reader);
                        break;

                    case "document":
                        documentNote.Document = reader.TokenType is JsonTokenType.Null
                            ? 0
                            : reader.GetInt32();
                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            return documentNote;
        }

        private static int? ReadUser(ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Null:
                    return null;

                case JsonTokenType.Number:
                    return reader.GetInt32();

                case JsonTokenType.StartObject:
                    int? userId = null;

                    while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                    {
                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            continue;
                        }

                        bool isId = reader.GetString() == "id";
                        reader.Read();

                        if (isId && reader.TokenType is JsonTokenType.Number)
                        {
                            userId = reader.GetInt32();
                        }
                        else
                        {
                            reader.Skip();
                        }
                    }

                    return userId;

                default:
                    reader.Skip();

                    return null;
            }
        }

        // Notizen werden nie in dieser Form geschrieben -- der POST-Rumpf ist
        // PaperlessNote. Die Implementierung bleibt trotzdem vollstaendig, damit der
        // Converter sich nicht anders verhaelt als er aussieht.
        public override void Write(
            Utf8JsonWriter writer,
            DocumentNote value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("id", value.Id);
            writer.WriteString("note", value.Note);
            writer.WriteString("created", value.Created);

            if (value.User is int user)
            {
                writer.WriteNumber("user", user);
            }
            else
            {
                writer.WriteNull("user");
            }

            writer.WriteNumber("document", value.Document);
            writer.WriteEndObject();
        }
    }
}
