using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TodoApi.SyncServices.ExternalAPI.Contracts
{
    public class ExternalTodoItem
    {
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("is_complete")]
        public bool IsComplete { get; set; }
    }
}
