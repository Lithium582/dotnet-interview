using Microsoft.AspNetCore.Mvc;
using TodoApi.Services.Services;
using TodoApi.SyncServices.Services;

namespace TodoApi.Controllers
{
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;
        private readonly ITodoItemService _todoItemService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            bool res = await _syncService.SyncTodoListsAsync();

            return Ok("Sync succeeded.");
        }
    }
}
