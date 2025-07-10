using Microsoft.AspNetCore.Mvc;
using TodoApi.Services.Services;
using TodoApi.SyncServices.Services;

namespace TodoApi.Controllers
{
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;
        private readonly TodoItemService _todoItemService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            var localTodoItems = _todoItemService.GetTodoItemsAsync(-1);
            //var externalTodoItems = _syncService.GetAllAsync();

            bool res = await _syncService.SyncTodoItemsAsync();

            return Ok("Sync succeeded.");
        }
    }
}
