using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TodoApi.Data.Models;
using TodoApi.Services.Dtos;

namespace TodoApi.Controllers
{
    [Route("api/listtodoitems/{listId}")]
    [ApiController]
    public class TodoItemController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoItemController(TodoContext context)
        {
            _context = context;
        }

        [HttpGet("GetItems")]
        public async Task<ActionResult<IList<TodoList>>> GetTodoItems(long listId)
        {
            var todoItems = await _context.TodoItem.Where((item) => item.ListId == listId).ToListAsync();

            if (todoItems == null)
            {
                return NotFound();
            }

            return Ok(todoItems);
        }

        [HttpGet("GetItem/{itemId}")]
        public async Task<ActionResult<TodoList>> GetTodoItem(long listId, long itemId)
        {
            var todoItem = await _context.TodoItem.FirstAsync((item) => item.ListId == listId && item.Id == itemId);

            if (todoItem == null)
            {
                return NotFound();
            }

            return Ok(todoItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> PostTodoItem(CreateTodoItem payload, long listId)
        {
            var todoItem = new TodoItem { ListId = listId, Title = payload.Title, Description = payload.Description };

            _context.TodoItem.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{itemId}")]
        public async Task<ActionResult> PutTodoItem(long listId, long itemId, UpdateTodoItem payload)
        {
            var todoItem = await _context.TodoItem.FirstAsync((item) => item.ListId == listId && item.Id == itemId);

            if (todoItem == null)
            {
                return NotFound();
            }

            todoItem.Title = payload.Title;
            todoItem.Description = payload.Description;
            todoItem.Completed = payload.Completed;

            await _context.SaveChangesAsync();

            return Ok(todoItem);
        }

        [HttpDelete("{itemId}")]
        public async Task<ActionResult> DeleteTodoList(long listId, long itemId)
        {
            var todoItem = await _context.TodoItem.FirstAsync((item) => item.ListId == listId && item.Id == itemId);

            if (todoItem == null)
            {
                return NotFound();
            }

            _context.TodoItem.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
