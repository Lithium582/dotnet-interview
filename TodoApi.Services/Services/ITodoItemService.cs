using TodoApi.Services.Dtos;

namespace TodoApi.Services.Services
{
    public interface ITodoItemService
    {
        Task<IList<TodoItemDto>> GetTodoItemsAsync(long listId);
        Task<TodoItemDto> GetTodoItemAsync(long listId, long itemId);
        Task<TodoItemDto> CreateTodoItemAsync(UpdateTodoItemDto todoItemDto, long listId);
        Task<bool> UpdateTodoItemAsync(UpdateTodoItemDto todoItemDto, long listId, long itemId);
        Task<bool> DeleteTodoItemAsync(long listId, long itemId);
    }
}
