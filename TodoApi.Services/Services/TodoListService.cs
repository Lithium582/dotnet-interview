using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data.Models;
using TodoApi.Services.Dtos;

namespace TodoApi.Services.Services
{
    public class TodoListService : ITodoListService
    {
        private readonly TodoContext _context;
        private readonly IMapper _mapper;

        public TodoListService(TodoContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<TodoListDto>> GetTodoListsAsync(bool includeDeleted = false)
        {
            var todoLists =
                await _context.TodoList
                .Include(l => l.Items.Where(i => !i.Deleted || includeDeleted))
                .Where(l => !l.Deleted || includeDeleted)
                .ToListAsync();

            return _mapper.Map<IList<TodoListDto>>(todoLists);
        }

        public async Task<TodoListDto> GetTodoListAsync(long listId)
        {
            var todoList = await _context.TodoList
                .Include((l) => l.Items.Where(i => !i.Deleted))
                .Where((l) => l.Id == listId && !l.Deleted)
                .FirstOrDefaultAsync();

            return _mapper.Map<TodoListDto>(todoList);
        }

        public async Task<TodoListDto> CreateTodoListAsync(UpdateTodoListDto todoListDto)
        {
            var todoList = _mapper.Map<TodoList>(todoListDto);
            todoList.UpdatedAt = DateTime.Now;
            todoList.CreatedAt = DateTime.Now;
            todoList.Deleted = false;

            _context.TodoList.Add(todoList);
            await _context.SaveChangesAsync();

            return _mapper.Map<TodoListDto>(todoList);
        }

        public async Task<bool> UpdateTodoListAsync(long listId, UpdateTodoListDto todoListDto)
        {
            var todoList = await _context.TodoList.FirstOrDefaultAsync((l) => l.Id == listId && !l.Deleted);

            if (todoList != null)
            {
                todoList.Name = todoListDto.Name;
                todoList.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteTodoListAsync(long listId)
        {
            var todoList = await _context.TodoList.FirstOrDefaultAsync((l) => l.Id == listId && !l.Deleted);

            if (todoList != null)
            {
                //_context.TodoList.Remove(todoList);
                todoList.Deleted = true;
                todoList.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
