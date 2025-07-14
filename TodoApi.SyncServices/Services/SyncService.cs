using AutoMapper;
using Microsoft.Extensions.Logging;
using TodoApi.ExternalContracts.Contracts;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.SyncServices.Services
{
    public class SyncService : ISyncService
    {
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

        public async Task<SyncResult> SyncTodoListsAsync()
        {
            try
            {
                // Getting lists from local
                var localLists = await _localListService.GetTodoListsAsync(true);

                // Getting lists from external API
                var externalLists = await _externalApi.GetTodoListsAsync();

                SyncResult fromExternalToLocal = await SyncFromExternalToLocal(localLists, externalLists);
                SyncResult fromLocalToExternal = await SyncFromLocalToExternal(localLists, externalLists);

                return (new SyncResult
                {
                    ItemCreations = fromExternalToLocal.ItemCreations + fromLocalToExternal.ItemCreations,
                    ItemUpdates = fromExternalToLocal.ItemUpdates + fromLocalToExternal.ItemUpdates,
                    ItemDeleted = fromExternalToLocal.ItemDeleted + fromLocalToExternal.ItemDeleted,
                    ListCreations = fromExternalToLocal.ListCreations + fromLocalToExternal.ListCreations,
                    ListUpdates = fromExternalToLocal.ListUpdates + fromLocalToExternal.ListUpdates,
                    ListDeleted = fromExternalToLocal.ListDeleted + fromLocalToExternal.ListDeleted
                });
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
                    var existing = localLists.FirstOrDefault(l => l.Id.ToString() == externalList.SourceId || l.ExternalId == externalList.Id);

                    if (existing == null) //External list does not exist on local
                    {
                        syncResult.ListCreations++;
                        var toCreate = _mapper.Map<UpdateTodoListDto>(externalList);
                        existing = await _localListService.CreateTodoListAsync(toCreate);

                        localLists.Add(existing);
                    }
                    else if (ListChanged(existing, externalList))
                    {
                        syncResult.ListUpdates++;
                        existing.Name = externalList.Name;

                        var toUpdate = _mapper.Map<UpdateTodoListDto>(existing);
                        await _localListService.UpdateTodoListAsync(existing.Id, toUpdate);
                    }
                    else
                    {
                        foreach (var externalItem in externalList.TodoItems)
                        {
                            try
                            {
                                var localItems = existing.Items ?? new List<TodoItemDto>();
                                var matchingItem = localItems.FirstOrDefault(i => i.Id.ToString() == externalItem.SourceId || i.ExternalId == externalItem.Id);

                                if (matchingItem == null)
                                {
                                    syncResult.ItemCreations++;
                                    var toCreate = _mapper.Map<UpdateTodoItemDto>(externalItem);
                                    toCreate.ListId = existing?.Id ?? 0;
                                    await _localItemService.CreateTodoItemAsync(toCreate, toCreate.ListId);
                                }
                                else if (ItemChanged(matchingItem, externalItem))
                                {
                                    syncResult.ItemUpdates++;
                                    var toUpdate = _mapper.Map<UpdateTodoItemDto>(externalItem);
                                    toUpdate.ListId = matchingItem.ListId;
                                    toUpdate.Title = matchingItem.Title;

                                    await _localItemService.UpdateTodoItemAsync(toUpdate, toUpdate.ListId, matchingItem.Id);
                                }
                            }
                            catch (Exception itemEx)
                            {
                                _logger.LogError(itemEx, $"Error syncing external item {externalItem.Id} in list {externalList.Id}");
                            }
                        }
                    }   
                }
                catch(Exception listEx)
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
                    var extList = externalLists.FirstOrDefault(el => el.SourceId == localList.Id.ToString() || localList.ExternalId == el.Id);

                    if (extList == null) //List does not exist on external
                    {
                        if (!String.IsNullOrEmpty(localList.ExternalId)) //List was already synced to External. So, if it does not exist, it means it was deleted on external
                        {
                            await _localListService.DeleteTodoListAsync(localList.Id);
                        }
                        else if (!localList.Deleted) //Checks if list was deleted previously, so it does not create it again
                        {
                            syncResult.ListCreations++;
                            var createDto = _mapper.Map<CreateExternalTodoList>(localList);
                            extList = await _externalApi.CreateTodoListAsync(createDto);

                            //Updates ExternalId on local
                            var toUpdate = _mapper.Map<UpdateTodoListDto>(extList);
                            await _localListService.UpdateTodoListAsync(localList.Id, toUpdate);
                        }
                    }
                    else if (localList.Deleted) //List was deleted on local but still exists on external
                    {
                        await _externalApi.DeleteTodoListAsync(localList.ExternalId);
                    }
                    else
                    {
                        if (ListChanged(localList, extList)) //Updates list just if there are changes on name
                        {
                            syncResult.ListUpdates++;

                            var updateListDto = _mapper.Map<UpdateExternalTodoList>(localList);
                            await _externalApi.UpdateTodoListAsync(extList.Id, updateListDto);
                        }

                        foreach (var localItem in localList.Items)
                        {   
                            try
                            {
                                var externalItems = extList.TodoItems ?? new List<ExternalTodoItem>(); //Checks for the items of existing lists
                                var extItem = extList.TodoItems.FirstOrDefault(ei => ei.SourceId == localItem.Id.ToString());

                                if (extItem == null)
                                {
                                    if (!String.IsNullOrEmpty(localItem.ExternalId)) //Item was already synced to External. So, if it does not exist, it means it was deleted on external
                                    {
                                        await _localItemService.DeleteTodoItemAsync(localList.Id, localItem.Id);
                                    }
                                    else if (!localItem.Deleted) //Checks if list was deleted previously, so it does not create it again
                                    {
                                        syncResult.ItemCreations++;

                                        var createItem = _mapper.Map<UpdateExternalTodoItem>(localItem);
                                        extItem = await _externalApi.UpdateTodoItemAsync(extList.Id, localItem.Id.ToString(), createItem);

                                        var toUpdate = _mapper.Map<UpdateTodoItemDto>(extItem); //Updates ExternalId on local
                                        toUpdate.Title = localItem.Title;

                                        await _localItemService.UpdateTodoItemAsync(toUpdate, localList.Id, localItem.Id);
                                    }
                                }
                                else if (localItem.Deleted) //Item was deleted on local but still exists on external
                                {
                                    await _externalApi.DeleteTodoItemAsync(localList.ExternalId, localItem.ExternalId);
                                }
                                else
                                {
                                    if (ItemChanged(localItem, extItem))
                                    {
                                        syncResult.ItemUpdates++;

                                        //Updates item on External
                                        var updateItemDto = _mapper.Map<UpdateExternalTodoItem>(localItem);
                                        await _externalApi.UpdateTodoItemAsync(extList.Id, extItem.Id, updateItemDto);
                                    }
                                }
                            }
                            catch(Exception itemEx)
                            {
                                _logger.LogError(itemEx, $"Error syncing local item {localItem.Id} in list {localList.Id}");
                            }
                        }
                    }
                }
                catch (Exception listEx)
                {
                    _logger.LogError(listEx, $"Error syncing local list {localList.Id} to external API");
                }
            }

            return syncResult;
        }

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

        #endregion
    }
}
