namespace TodoApi.Services.Dtos
{
    public class UpdateTodoItemDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required bool Completed { get; set; }
        public long ListId { get; set; }
        public string? ExternalId { get; set; }
    }
}
