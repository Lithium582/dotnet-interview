using TodoApi.SyncServices.ExternalAPI.Contracts;

namespace TodoApi.SyncServices.ExternalAPI.Auxiliars
{
    public static class FakeDataGenerator
    {
        public static List<ExternalTodoList> GenerateListsWithItems()
        {
            return new List<ExternalTodoList>
        {
            new ExternalTodoList
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Lista A",
                TodoItems = new List<ExternalTodoItem>
                {
                    new ExternalTodoItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        //Title = "Item A1",
                        Description = "Primera tarea",
                        Completed = false
                    },
                    new ExternalTodoItem
                    {
                        Id = Guid.NewGuid().ToString(),
                        //Title = "Item A2",
                        Description = "Segunda tarea",
                        Completed = true
                    }
                }
            },
            new ExternalTodoList
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Lista B",
                TodoItems = new List<ExternalTodoItem>()
            }
        };
        }
    }
}
