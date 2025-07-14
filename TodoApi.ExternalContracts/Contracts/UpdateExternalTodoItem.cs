using System.Text.Json.Serialization;

namespace TodoApi.ExternalContracts.Contracts
{
    public class UpdateExternalTodoItem
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
    }
}
