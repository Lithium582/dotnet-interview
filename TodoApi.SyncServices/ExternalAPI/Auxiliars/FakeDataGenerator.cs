using TodoApi.ExternalContracts.Contracts;

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
                Id = "b71f6cc9-f684-422a-905e-23395fdd117f",
                Name = "Lista A1",
                TodoItems = new List<ExternalTodoItem>
                {
                    new ExternalTodoItem
                    {
                        Id = "9c3e6228-f9b2-4c77-a359-8e7a74f6c4fe",
                        Description = "Primera tarea",
                        Completed = false
                    },
                    new ExternalTodoItem
                    {
                        Id = "fddc03b5-deb7-40dd-9306-7ea532e30f01",
                        Description = "Segunda tarea",
                        Completed = true
                    }
                }
            },
            new ExternalTodoList
            {
                Id = "d3d7aada-5910-4315-97fe-b6f559a8de41",
                Name = "Lista B",
                TodoItems = new List<ExternalTodoItem>()
            }
        };
        }
    }
}
