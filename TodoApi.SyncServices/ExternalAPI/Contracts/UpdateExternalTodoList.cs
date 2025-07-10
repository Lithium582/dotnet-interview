using System.Text.Json.Serialization;

namespace TodoApi.SyncServices.ExternalAPI.Contracts
{
    public class UpdateExternalTodoList
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
