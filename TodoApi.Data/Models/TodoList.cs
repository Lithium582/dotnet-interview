namespace TodoApi.Data.Models;

public class TodoList
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public List<TodoItem> Items { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool Deleted { get; set; }
    public string? ExternalId { get; set; }
}
