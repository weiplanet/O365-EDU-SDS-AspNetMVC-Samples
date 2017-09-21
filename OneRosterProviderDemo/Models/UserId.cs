using Newtonsoft.Json;

namespace OneRosterProviderDemo.Models
{
    public class UserId
    {
        public string Type { get; set; }
        public string Identifier { get; set; }

        public void AsJson(JsonWriter writer, string baseUrl)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue(Type);
            writer.WritePropertyName("identifier");
            writer.WriteValue(Identifier);

            writer.WriteEndObject();
            writer.Flush();
        }
    }
}
