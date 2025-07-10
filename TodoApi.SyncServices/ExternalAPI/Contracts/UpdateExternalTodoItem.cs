using System.Text.Json.Serialization;

namespace TodoApi.SyncServices.ExternalAPI.Contracts
{
    public class UpdateExternalTodoItem
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("completed")]
        public bool Completed { get; set; }
    }
}
