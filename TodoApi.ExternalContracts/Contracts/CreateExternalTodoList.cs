using System.Text.Json.Serialization;

namespace TodoApi.ExternalContracts.Contracts
{
    public class CreateExternalTodoList
    {
        [JsonPropertyName("source_id")]
        public string SourceId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("items")]
        public List<ExternalTodoItem> TodoItems { get; set; }
    }
}
