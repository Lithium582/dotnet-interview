namespace TodoApi.Services.Dtos
{
    public class TodoListDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<TodoItemDto> Items { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool Deleted { get; set; }
        public string ExternalId { get; set; }
    }
}
