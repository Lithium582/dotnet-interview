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

        #region "Lists"

        public async Task<List<ExternalTodoList>> GetTodoListsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<ExternalTodoList> CreateTodoListAsync(CreateExternalTodoList dto)
        {
            throw new NotImplementedException();

        }

        public async Task<ExternalTodoList> UpdateTodoListAsync(string listId, UpdateExternalTodoList dto)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteTodoListAsync(string listId)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region "Items"

        public async Task<ExternalTodoItem> UpdateTodoItemAsync(string listId, string itemId, UpdateExternalTodoItem dto)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteTodoItemAsync(string listId, string itemId)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
