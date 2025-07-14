using Microsoft.AspNetCore.Mvc;
using TodoApi.Services.Services;
using TodoApi.SyncServices.Services;

namespace TodoApi.Controllers
{
    [Route("api/sync")]
    [ApiController]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost]
        public async Task<IActionResult> Sync()
        {
            var res = await _syncService.SyncTodoListsAsync();

            return Ok("Sync succeeded.");
        }
    }
}
