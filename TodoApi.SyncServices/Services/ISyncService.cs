using TodoApi.ExternalContracts.Contracts;

namespace TodoApi.SyncServices.Services
{
    public interface ISyncService
    {
        Task<SyncResult> SyncTodoListsAsync();
    }
}
