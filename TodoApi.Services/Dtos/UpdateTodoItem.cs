namespace TodoApi.Services.Dtos
{
    public class UpdateTodoItem
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required bool Completed { get; set; }
    }
}
