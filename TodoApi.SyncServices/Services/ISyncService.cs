using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApi.SyncServices.Services
{
    public interface ISyncService
    {
        Task<bool> SyncTodoItemsAsync();
    }
}
