using Microsoft.AspNetCore.Mvc;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.Controllers
{
    [Route("api/listTodoItems/{listId}")]
    [ApiController]
    public class TodoItemController : ControllerBase
    {
        private readonly ITodoItemService _service;

        public TodoItemController(ITodoItemService service)
        {
            _service = service;
        }

        [HttpGet("GetTodoItems")]
        public async Task<ActionResult<IList<TodoItemDto>>> GetTodoItems(long listId)
        {
            var todoItems = await _service.GetTodoItemsAsync(listId);

            if (todoItems == null)
            {
                return NotFound();
            }

            return Ok(todoItems);
        }

        [HttpGet("GetTodoItem/{itemId}")]
        public async Task<ActionResult<TodoListDto>> GetTodoItem(long listId, long itemId)
        {
            var todoItem = await _service.GetTodoItemAsync(listId, itemId);

            if (todoItem == null)
            {
                return NotFound();
            }

            return Ok(todoItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItemDto>> CreateTodoItemAsync(UpdateTodoItemDto payload, long listId)
        {
            var todoItem = await _service.CreateTodoItemAsync(payload, listId);

            return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{itemId}")]
        public async Task<ActionResult<bool>> PutTodoItem(UpdateTodoItemDto payload, long listId, long itemId)
        {
            var updated = await _service.UpdateTodoItemAsync(payload, listId, itemId);

            if (updated == false)
            {
                return NotFound();
            }

            return Ok(true);
        }

        [HttpDelete("{itemId}")]
        public async Task<ActionResult<bool>> DeleteTodoList(long listId, long itemId)
        {
            var deleted = await _service.DeleteTodoItemAsync(listId, itemId);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
