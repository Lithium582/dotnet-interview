using Microsoft.AspNetCore.Mvc;
using TodoApi.SyncServices.Services;

namespace TodoApi.Controllers
{
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        //[HttpPost]
        //public async Task<IActionResult> Sync()
        //{
        //    await _syncService.SyncTodoListsAsync(); // y/o .SyncTodoItemsAsync()
        //    return Ok("Sincronización completada.");
        //}
    }
}
