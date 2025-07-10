using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApi.SyncServices.ExternalAPI.Contracts;

namespace TodoApi.SyncServices.Services
{
    public class SyncService : ISyncService
    {
        private readonly IExternalAPI _externalAPI;
        public SyncService(IExternalAPI externalAPI)
        {
            _externalAPI = externalAPI;
        }

        public async Task<List<ExternalTodoItem>> GetAllExternalItems()
        {
            return null;
        }

        public async Task<bool> SyncTodoItemsAsync()
        {   
            return true;
        }
    }
}
