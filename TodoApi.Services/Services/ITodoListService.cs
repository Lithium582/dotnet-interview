using TodoApi.Services.Dtos;

namespace TodoApi.Services.Services
{
    public interface ITodoListService
    {
        Task<IList<TodoListDto>> GetTodoListsAsync();
        Task<TodoListDto> GetTodoListAsync(long listId);
        Task<TodoListDto> CreateTodoListAsync(UpdateTodoListDto todoListDto);
        Task<bool> UpdateTodoListAsync(long listId, UpdateTodoListDto todoListDto);
        Task<bool> DeleteTodoListAsync(long listId);
    }
}
