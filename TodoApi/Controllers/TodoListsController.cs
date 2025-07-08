using Microsoft.AspNetCore.Mvc;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.Controllers
{
    [Route("api/todolists")]
    [ApiController]
    public class TodoListsController : ControllerBase
    {
        private readonly ITodoListService _service;

        public TodoListsController(ITodoListService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IList<TodoListDto>>> GetTodoLists()
        {
            var todoLists = await _service.GetTodoListsAsync();

            if (todoLists == null)
            {
                return NotFound();
            }

            return Ok(todoLists);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoListDto>> GetTodoList(long id)
        {
            var todoList = await _service.GetTodoListAsync(id);

            if (todoList == null)
            {
                return NotFound();
            }

            return Ok(todoList);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<bool>> PutTodoList(long id, UpdateTodoListDto payload)
        {
            var updated = await _service.UpdateTodoListAsync(id, payload);

            if (updated == false)
            {
                return NotFound();
            }

            return Ok(true);
        }

        [HttpPost]
        public async Task<ActionResult<TodoListDto>> PostTodoList(UpdateTodoListDto payload)
        {
            var todoList = await _service.CreateTodoListAsync(payload);

            return CreatedAtAction("GetTodoList", new { id = todoList.Id }, todoList);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> DeleteTodoList(long id)
        {
            var deleted = await _service.DeleteTodoListAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        //private bool TodoListExists(long id)
        //{
        //    return (_context.TodoList?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
