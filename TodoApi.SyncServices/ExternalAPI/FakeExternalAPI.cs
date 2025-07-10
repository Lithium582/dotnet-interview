using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApi.SyncServices.ExternalAPI.Contracts;

namespace TodoApi.SyncServices.ExternalAPI
{
    public class FakeExternalAPI : IExternalAPI
    {
        private readonly List<ExternalTodoItem> _fakeStorage;

        public FakeExternalAPI()
        {
            _fakeStorage = new List<ExternalTodoItem>
        {
            new ExternalTodoItem { ExternalId = "abc123", Name = "External Task A", IsComplete = false },
            new ExternalTodoItem { ExternalId = "def456", Name = "External Task B", IsComplete = true }
        };
        }

        public Task<List<ExternalTodoItem>> GetAllAsync()
        {
            // Fakes a GET to /external/todo
            return Task.FromResult(_fakeStorage.ToList());
        }

        public Task SyncAsync(List<ExternalTodoItem> items)
        {
            // Fakes a POST to /external/todo/sync

            foreach (var item in items)
            {
                var existing = _fakeStorage.FirstOrDefault(x => x.ExternalId == item.ExternalId);
                if (existing != null)
                {
                    existing.Name = item.Name;
                    existing.IsComplete = item.IsComplete;
                }
                else
                {
                    _fakeStorage.Add(item);
                }
            }

            return Task.CompletedTask;
        }
    }
}
