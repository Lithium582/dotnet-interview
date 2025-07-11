namespace TodoApi.SyncServices.Services
{
    public interface ISyncService
    {
        Task<bool> SyncTodoListsAsync();
    }
}
