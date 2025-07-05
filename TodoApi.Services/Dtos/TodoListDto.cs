using TodoApi.Data.Models;

namespace TodoApi.Services.Dtos
{
    public class TodoListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<TodoItemDto> Items { get; set; }
    }
}
