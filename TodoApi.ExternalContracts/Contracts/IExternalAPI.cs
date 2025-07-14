using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TodoApi.ExternalContracts.Contracts
{
    public interface IExternalAPI
    {
        //Lists
        Task<IList<ExternalTodoList>> GetTodoListsAsync();
        Task<ExternalTodoList> CreateTodoListAsync(CreateExternalTodoList dto);
        Task<ExternalTodoList> UpdateTodoListAsync(string listId, UpdateExternalTodoList dto);
        Task DeleteTodoListAsync(string listId);


        // Items
        Task<ExternalTodoItem> UpdateTodoItemAsync(string listId, string itemId, UpdateExternalTodoItem dto);
        Task DeleteTodoItemAsync(string listId, string itemId);
    }
}
