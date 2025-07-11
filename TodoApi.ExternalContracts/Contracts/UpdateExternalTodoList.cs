using System.Text.Json.Serialization;

namespace TodoApi.ExternalContracts.Contracts
{
    public class UpdateExternalTodoList
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
