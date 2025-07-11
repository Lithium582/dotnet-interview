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
                // 1. Obtener datos de ambas fuentes
                var localLists = await _localListService.GetTodoListsAsync(); // List<TodoListDto>
                var externalLists = await _externalApi.GetTodoListsAsync();   // List<ExternalTodoList>

                // 2. Sincronizar listas de la API externa a la BD local
                foreach (var externalList in externalLists)
                {
                    var existing = localLists.FirstOrDefault(l => l.Id.ToString() == externalList.SourceId);

                    if (existing == null)
                    {
                        // Crear nueva lista local
                        var toCreate = new UpdateTodoListDto { Name = externalList.Name };
                        await _localListService.CreateTodoListAsync(toCreate);
                    }
                    else if (existing.Name != externalList.Name)
                    {
                        // Actualizar lista local
                        var toUpdate = new UpdateTodoListDto { Name = externalList.Name };
                        await _localListService.UpdateTodoListAsync(existing.Id, toUpdate);
                    }

                    // 3. Sincronizar items de esta lista
                    foreach (var externalItem in externalList.TodoItems)
                    {
                        var localItems = await _localItemService.GetTodoItemsAsync(existing?.Id ?? 0);
                        var matchingItem = localItems.FirstOrDefault(i => i.Id.ToString() == externalItem.SourceId);

                        if (matchingItem == null)
                        {
                            // Crear nuevo item
                            var toCreate = _mapper.Map<UpdateTodoItemDto>(externalItem);
                            toCreate.ListId = existing?.Id ?? 0;
                            await _localItemService.CreateTodoItemAsync(toCreate, toCreate.ListId);
                        }
                        else if (matchingItem.Description != externalItem.Description ||
                                 matchingItem.Completed != externalItem.Completed)
                        {
                            // Actualizar item
                            var toUpdate = _mapper.Map<UpdateTodoItemDto>(externalItem);
                            toUpdate.ListId = matchingItem.ListId;
                            await _localItemService.UpdateTodoItemAsync(toUpdate, toUpdate.ListId, matchingItem.Id);
                        }
                    }
                }

                // 4. (opcional) Detectar si hay listas en local que no están en la externa y subirlas

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
