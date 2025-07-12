using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public SyncService(ITodoListService localListService, ITodoItemService localItemService, IExternalAPI externalApi, IMapper mapper)
        {
            _localListService = localListService;
            _localItemService = localItemService;
            _externalApi = externalApi;
            _mapper = mapper;
        }

        public async Task<bool> SyncTodoListsAsync()
        {
            try
            {
                // Getting lists from local
                var localLists = await _localListService.GetTodoListsAsync();

                // Getting lists from external API
                var externalLists = await _externalApi.GetTodoListsAsync();

                foreach (var externalList in externalLists)
                {
                    var existing = localLists.FirstOrDefault(l => l.Id.ToString() == externalList.SourceId);

                    if (existing == null)
                    {
                        var toCreate = new UpdateTodoListDto { Name = externalList.Name };
                        await _localListService.CreateTodoListAsync(toCreate);
                    }
                    else if (existing.Name != externalList.Name)
                    {
                        var toUpdate = new UpdateTodoListDto { Name = externalList.Name };
                        await _localListService.UpdateTodoListAsync(existing.Id, toUpdate);
                    }

                    foreach (var externalItem in externalList.TodoItems)
                    {
                        var localItems = await _localItemService.GetTodoItemsAsync(existing?.Id ?? 0);
                        var matchingItem = localItems.FirstOrDefault(i => i.Id.ToString() == externalItem.SourceId);

                        if (matchingItem == null)
                        {
                            var toCreate = _mapper.Map<UpdateTodoItemDto>(externalItem);
                            toCreate.ListId = existing?.Id ?? 0;
                            await _localItemService.CreateTodoItemAsync(toCreate, toCreate.ListId);
                        }
                        else if (matchingItem.Description != externalItem.Description ||
                                 matchingItem.Completed != externalItem.Completed)
                        {
                            var toUpdate = _mapper.Map<UpdateTodoItemDto>(externalItem);
                            toUpdate.ListId = matchingItem.ListId;
                            await _localItemService.UpdateTodoItemAsync(toUpdate, toUpdate.ListId, matchingItem.Id);
                        }
                    }
                }

                // 5. Propagar creaciones y actualizaciones locales hacia la API externa
                foreach (var localList in localLists)
                {
                    // ¿Existe esta lista en la externa?
                    var extList = externalLists.FirstOrDefault(el => el.SourceId == localList.Id.ToString());

                    if (extList == null)
                    {
                        // Crear lista en la externa
                        var createDto = _mapper.Map<CreateExternalTodoList>(localList);
                        await _externalApi.CreateTodoListAsync(createDto);
                    }
                    else if (extList.Name != localList.Name)
                    {
                        // Actualizar lista en la externa
                        var updateDto = _mapper.Map<UpdateExternalTodoList>(localList);
                        await _externalApi.UpdateTodoListAsync(extList.Id, updateDto);
                    }

                    // Ahora los ítems de esta lista
                    var localItems = await _localItemService.GetTodoItemsAsync(localList.Id);
                    foreach (var localItem in localItems)
                    {
                        var extItem = extList?.TodoItems.FirstOrDefault(ei => ei.SourceId == localItem.Id.ToString());

                        if (extItem == null)
                        {
                            // No existe externo → usamos PUT como upsert
                            var upsertDto = _mapper.Map<UpdateExternalTodoItem>(localItem);
                            await _externalApi.UpdateTodoItemAsync(extList.Id, localItem.Id.ToString(), upsertDto);
                        }
                        else if (extItem.Description != localItem.Description ||
                                 extItem.Completed != localItem.Completed)
                        {
                            // Actualizar ítem existente
                            var updateDto = _mapper.Map<UpdateExternalTodoItem>(localItem);
                            await _externalApi.UpdateTodoItemAsync(extList.Id, extItem.Id, updateDto);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // TODO: loggear el error
                return false;
            }
        }
    }
}
