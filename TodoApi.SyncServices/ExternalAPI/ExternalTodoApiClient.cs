using TodoApi.SyncServices.ExternalAPI.Contracts;

namespace TodoApi.SyncServices.ExternalAPI
{
    public class ExternalTodoApiClient : IExternalAPI
    {
        private readonly HttpClient _httpClient;

        public ExternalTodoApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<List<ExternalTodoItem>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task SyncAsync(List<ExternalTodoItem> items)
        {
            throw new NotImplementedException();
        }
    }
}
