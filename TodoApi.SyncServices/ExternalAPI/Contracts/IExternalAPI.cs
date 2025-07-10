using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApi.SyncServices.ExternalAPI.Contracts
{
    public interface IExternalAPI
    {
        Task<List<ExternalTodoItem>> GetAllAsync();
        Task SyncAsync(List<ExternalTodoItem> items);
    }
}
