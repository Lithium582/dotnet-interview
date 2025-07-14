namespace TodoApi.Data.Models;

public class TodoItem
{
    public TodoList? List { get; set; }
    public long ListId { get; set; }
    public long Id { get; set; }
    public bool Completed { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool Deleted { get; set; }
    public string? ExternalId { get; set; }

}
