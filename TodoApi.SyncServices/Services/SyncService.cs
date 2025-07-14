using AutoMapper;
using Microsoft.Extensions.Logging;
using TodoApi.ExternalContracts.Contracts;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.SyncServices.Services
{
    public class SyncService : ISyncService
    {
        #region "Constructor and initialization"
        private readonly ITodoListService _localListService;
        private readonly ITodoItemService _localItemService;
        private readonly IExternalAPI _externalApi;
        private readonly IMapper _mapper;
        private readonly ILogger<SyncService> _logger;

        public SyncService(
            ITodoListService localListService,
            ITodoItemService localItemService,
            IExternalAPI externalApi,
            IMapper mapper,
            ILogger<SyncService> logger)
        {
            _localListService = localListService;
            _localItemService = localItemService;
            _externalApi = externalApi;
            _mapper = mapper;
            _logger = logger;
        }

        #endregion

        #region "Main methods"
        public async Task<SyncResult?> SyncTodoListsAsync()
        {
            try
            {
                // Getting lists from local
                var localLists = await _localListService.GetTodoListsAsync(includeDeleted: true);

                // Getting lists from external API
                var externalLists = await _externalApi.GetTodoListsAsync();

                SyncResult externalToLocal = await SyncFromExternalToLocal(localLists, externalLists);
                SyncResult localToExternal = await SyncFromLocalToExternal(localLists, externalLists);

                return CombineResults(externalToLocal, localToExternal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error during Sync process");
                return null;
            }
        }

        private async Task<SyncResult> SyncFromExternalToLocal(
            IList<TodoListDto> localLists,
            IList<ExternalTodoList> externalLists)
        {
            SyncResult syncResult = new SyncResult();

            foreach (var externalList in externalLists)
            {
                try
                {
                    var localListExisting = FindLocalList(localLists, externalList);

                    if (localListExisting == null) //External list does not exist on local
                    {
                        localListExisting = await CreateLocalListFromExternal(externalList, syncResult);
                        localLists.Add(localListExisting);
                    }
                    else if (ListChanged(localListExisting, externalList))
                    {
                        await UpdateLocalListFromExternal(localListExisting, externalList, syncResult);
                    }

                    await SyncItemsFromExternalList(localListExisting, externalList.TodoItems, syncResult);
                }
                catch (Exception listEx)
                {
                    _logger.LogError(listEx, $"Error syncing external list {externalList.Id} to local");
                }
            }

            return syncResult;
        }

        private async Task<SyncResult> SyncFromLocalToExternal(IList<TodoListDto> localLists, IList<ExternalTodoList> externalLists)
        {
            SyncResult syncResult = new SyncResult();

            foreach (var localList in localLists)
            {
                try
                {
                    var extList = FindExternalList(externalLists, localList);

                    if (extList == null)
                    {
                        await HandleMissingExternalList(localList, syncResult);
                    }
                    else
                    {
                        await SyncLocalListToExternal(localList, extList, syncResult);
                    }   
                }
                catch (Exception listEx)
                {
                    _logger.LogError(listEx, $"Error syncing local list {localList.Id} to external API");
                }
            }

            return syncResult;
        }

        #endregion

        #region "Logic Aux methods - LocalFromExternal"

        private TodoListDto FindLocalList(IList<TodoListDto> localLists, ExternalTodoList externalList)
        {
            return localLists.FirstOrDefault(l =>
                l.Id.ToString() == externalList.SourceId || l.ExternalId == externalList.Id);
        }

        private async Task<TodoListDto> CreateLocalListFromExternal(ExternalTodoList externalList, SyncResult result)
        {
            var toCreate = _mapper.Map<UpdateTodoListDto>(externalList);
            var created = await _localListService.CreateTodoListAsync(toCreate);
            result.ListCreations++;
            return created;
        }

        private async Task UpdateLocalListFromExternal(TodoListDto local, ExternalTodoList external, SyncResult result)
        {
            local.Name = external.Name;
            var toUpdate = _mapper.Map<UpdateTodoListDto>(local);
            await _localListService.UpdateTodoListAsync(local.Id, toUpdate);
            result.ListUpdates++;
        }

        private async Task SyncItemsFromExternalList(TodoListDto localList, IList<ExternalTodoItem> externalItems, SyncResult result)
        {
            var localItems = localList.Items ?? new List<TodoItemDto>();

            foreach (var externalItem in externalItems)
            {
                try
                {
                    var localItem = FindLocalItem(localItems, externalItem);

                    if (localItem == null)
                    {
                        await CreateLocalItemFromExternal(localList.Id, externalItem, result);
                    }
                    else if (ItemChanged(localItem, externalItem))
                    {
                        await UpdateLocalItemFromExternal(localList.Id, localItem, externalItem, result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error syncing item {externalItem.Id} in list {localList.Id}");
                }
            }
        }

        private TodoItemDto FindLocalItem(IEnumerable<TodoItemDto> localItems, ExternalTodoItem externalItem)
        {
            return localItems.FirstOrDefault(i =>
                i.Id.ToString() == externalItem.SourceId || i.ExternalId == externalItem.Id);
        }

        private async Task CreateLocalItemFromExternal(long listId, ExternalTodoItem externalItem, SyncResult result)
        {
            var toCreate = _mapper.Map<UpdateTodoItemDto>(externalItem);
            toCreate.ListId = listId;
            await _localItemService.CreateTodoItemAsync(toCreate, listId);
            result.ItemCreations++;
        }

        private async Task UpdateLocalItemFromExternal(long listId, TodoItemDto localItem, ExternalTodoItem externalItem, SyncResult result)
        {
            var toUpdate = _mapper.Map<UpdateTodoItemDto>(externalItem);
            toUpdate.ListId = listId;
            toUpdate.Title = localItem.Title;
            await _localItemService.UpdateTodoItemAsync(toUpdate, listId, localItem.Id);
            result.ItemUpdates++;
        }

        #endregion

        #region "Logic Aux methods - ExternalToLocal"
        private ExternalTodoList FindExternalList(IEnumerable<ExternalTodoList> externalLists, TodoListDto localList)
        {
            return externalLists.FirstOrDefault(ext =>
                ext.SourceId == localList.Id.ToString() || ext.Id == localList.ExternalId);
        }

        private async Task HandleMissingExternalList(TodoListDto localList, SyncResult result)
        {
            if (!string.IsNullOrEmpty(localList.ExternalId)) //List was already synced to External. So, if it does not exist, it means it was deleted on external
            {
                await _localListService.DeleteTodoListAsync(localList.Id);
                result.ListDeleted++;
            }
            else if (!localList.Deleted) //Checks if list was deleted previously, so it does not create it again
            {
                var createDto = _mapper.Map<CreateExternalTodoList>(localList);
                var createdExternal = await _externalApi.CreateTodoListAsync(createDto);

                var updateDto = _mapper.Map<UpdateTodoListDto>(createdExternal); //Updates ExternalId on local
                await _localListService.UpdateTodoListAsync(localList.Id, updateDto);

                result.ListCreations++;
            }
        }

        private async Task SyncLocalListToExternal(TodoListDto localList, ExternalTodoList externalList, SyncResult result)
        {
            if (localList.Deleted) //List was deleted on local but still exists on external
            {
                await _externalApi.DeleteTodoListAsync(localList.ExternalId);
                result.ListDeleted++;
                return;
            }

            if (ListChanged(localList, externalList)) //Updates list just if there are changes on name
            {
                var updateDto = _mapper.Map<UpdateExternalTodoList>(localList);
                await _externalApi.UpdateTodoListAsync(externalList.Id, updateDto);
                result.ListUpdates++;
            }

            await SyncLocalItemsToExternal(localList, externalList, result);
        }

        private async Task SyncLocalItemsToExternal(TodoListDto localList, ExternalTodoList externalList, SyncResult result)
        {
            foreach (var localItem in localList.Items)
            {
                try
                {
                    var extItem = externalList.TodoItems.FirstOrDefault(ei =>
                        ei.SourceId == localItem.Id.ToString() || ei.Id == localItem.ExternalId);

                    if (extItem == null)
                    {
                        await HandleMissingExternalItem(localList, localItem, result);
                    }
                    else
                    {
                        await SyncExistingItemToExternal(localList, localItem, extItem, result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error syncing item {localItem.Id} in list {localList.Id}");
                }
            }
        }

        private async Task HandleMissingExternalItem(TodoListDto list, TodoItemDto item, SyncResult result)
        {
            if (!string.IsNullOrEmpty(item.ExternalId)) //Item was already synced to External. So, if it does not exist, it means it was deleted on external
            {
                await _localItemService.DeleteTodoItemAsync(list.Id, item.Id);
                result.ItemDeleted++;
            }
            else if (!item.Deleted) //Checks if list was deleted previously, so it does not create it again
            {
                var createDto = _mapper.Map<UpdateExternalTodoItem>(item);
                var createdExternal = await _externalApi.UpdateTodoItemAsync(list.ExternalId, item.Id.ToString(), createDto);

                var updateLocalDto = _mapper.Map<UpdateTodoItemDto>(createdExternal); //Updates ExternalId on local
                updateLocalDto.Title = item.Title;
                await _localItemService.UpdateTodoItemAsync(updateLocalDto, list.Id, item.Id);

                result.ItemCreations++;
            }
        }

        private async Task SyncExistingItemToExternal(TodoListDto list, TodoItemDto localItem, ExternalTodoItem externalItem, SyncResult result)
        {
            if (localItem.Deleted) //Item was deleted on local but still exists on external
            {
                await _externalApi.DeleteTodoItemAsync(list.ExternalId, localItem.ExternalId);
                result.ItemDeleted++;
            }
            else if (ItemChanged(localItem, externalItem))
            {
                var updateDto = _mapper.Map<UpdateExternalTodoItem>(localItem);
                await _externalApi.UpdateTodoItemAsync(list.ExternalId, externalItem.Id, updateDto);

                result.ItemUpdates++;
            }
        }

        #endregion

        #region "Aux methods"

        private bool ItemChanged(TodoItemDto local, ExternalTodoItem external)
        {
            return (local.Description != external.Description ||
                    local.Completed != external.Completed);
        }

        private bool ListChanged(TodoListDto local, ExternalTodoList external)
        {
            return (local.Name != external.Name);
        }

        private SyncResult CombineResults(SyncResult a, SyncResult b)
        {
            return (new SyncResult
            {
                ItemCreations = a.ItemCreations + b.ItemCreations,
                ItemUpdates = a.ItemUpdates + b.ItemUpdates,
                ItemDeleted = a.ItemDeleted + b.ItemDeleted,
                ListCreations = a.ListCreations + b.ListCreations,
                ListUpdates = a.ListUpdates + b.ListUpdates,
                ListDeleted = a.ListDeleted + b.ListDeleted
            });
        }
        #endregion
    }
}