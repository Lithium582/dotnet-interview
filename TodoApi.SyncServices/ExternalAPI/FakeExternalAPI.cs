using TodoApi.ExternalContracts.Contracts;
using TodoApi.SyncServices.ExternalAPI.Auxiliars;

namespace TodoApi.SyncServices.ExternalAPI
{
    public class FakeExternalAPI : IExternalAPI
    {
        private readonly List<ExternalTodoList> _fakeLists;

        public FakeExternalAPI()
        {
            _fakeLists = FakeDataGenerator.GenerateListsWithItems();
        }

        #region "Lists"

        public async Task<IList<ExternalTodoList>> GetTodoListsAsync()
        {
            return _fakeLists.ToList();
        }

        public async Task<ExternalTodoList> CreateTodoListAsync(CreateExternalTodoList dto)
        {
            var newList = new ExternalTodoList
            {
                Id = Guid.NewGuid().ToString(),
                SourceId = dto.SourceId,
                Name = dto.Name,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                TodoItems = new List<ExternalTodoItem>()
            };

            _fakeLists.Add(newList);
            return newList;

        }

        public async Task<ExternalTodoList> UpdateTodoListAsync(string listId, UpdateExternalTodoList dto)
        {
            var list = _fakeLists.FirstOrDefault(l => l.Id == listId);
            if (list == null) throw new Exception("List not found");

            list.Name = dto.Name;
            list.UpdatedAt = DateTime.Now;
            return list;
        }

        public async Task DeleteTodoListAsync(string listId)
        {
            var list = _fakeLists.FirstOrDefault(l => l.Id == listId);
            if (list != null)
                _fakeLists.Remove(list);

            //return Task.CompletedTask;
        }

        #endregion

        #region "Items"

        public async Task<ExternalTodoItem> UpdateTodoItemAsync(string listId, string itemId, UpdateExternalTodoItem dto)
        {
            var list = _fakeLists.FirstOrDefault(l => l.Id == listId);
            if (list == null) throw new Exception("List not found");

            var item = list.TodoItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null) throw new Exception("Item not found");

            //item.Title = dto.Title;
            item.Description = dto.Description;
            item.Completed = dto.Completed;
            item.UpdatedAt = DateTime.Now;

            return item;
        }

        public async Task DeleteTodoItemAsync(string listId, string itemId)
        {
            var list = _fakeLists.FirstOrDefault(l => l.Id == listId);
            //if (list == null) return Task.CompletedTask;

            var item = list.TodoItems.FirstOrDefault(i => i.Id == itemId);
            if (item != null)
                list.TodoItems.Remove(item);

            //return Task.CompletedTask;
        }

        #endregion
    }
}
