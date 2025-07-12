using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TodoApi.Data.Models;
using TodoApi.Services.Dtos;

namespace TodoApi.Services.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly TodoContext _context;
        private readonly IMapper _mapper;

        public TodoItemService(TodoContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<TodoItemDto>> GetTodoItemsAsync(long listId)
        {
            var todoItems = 
                await _context.TodoItem
                .Include(i => i.List)
                .Where(i  => i.ListId == listId && !i.Deleted).ToListAsync();

            return _mapper.Map<IList<TodoItemDto>>(todoItems);
        }

        public async Task<TodoItemDto> GetTodoItemAsync(long listId, long itemId)
        {
            var todoItem = await _context.TodoItem
                .Include((i) => i.List)
                .Where((i) => i.ListId == listId && i.Id == itemId && !i.Deleted)
                .FirstOrDefaultAsync();

            return _mapper.Map<TodoItemDto>(todoItem);
        }

        public async Task<TodoItemDto> CreateTodoItemAsync(UpdateTodoItemDto todoItemDto, long listId)
        {
            var todoItem = _mapper.Map<TodoItem>(todoItemDto);

            todoItem.UpdatedAt = DateTime.Now;
            todoItem.CreatedAt = DateTime.Now;
            todoItem.Deleted = false;

            _context.TodoItem.Add(todoItem);
            await _context.SaveChangesAsync();

            await _context.Entry(todoItem).Reference(e => e.List).LoadAsync();

            return _mapper.Map<TodoItemDto>(todoItem);
        }

        public async Task<bool> UpdateTodoItemAsync(UpdateTodoItemDto todoItemDto, long listId, long itemId)
        {
            var todoItem = await _context.TodoItem.FirstOrDefaultAsync(
                (i) => i.ListId == listId
                    && i.Id == itemId
                    && !i.Deleted
                );

            if (todoItem != null)
            {
                todoItem.Title = todoItemDto.Title;
                todoItem.Description = todoItemDto.Description;
                todoItem.Completed = todoItemDto.Completed;
                todoItem.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteTodoItemAsync(long listId, long itemId)
        {
            var todoItem = await _context.TodoItem.FirstOrDefaultAsync(
                (i) => i.ListId == listId
                    && i.Id == itemId
                    && !i.Deleted);

            if (todoItem != null)
            {
                //_context.TodoItem.Remove(todoItem);
                todoItem.Deleted = true;
                todoItem.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
