using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using TodoApi.ExternalContracts.Contracts;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;
using TodoApi.SyncServices.Services;

namespace TodoApi.Tests.Sync
{
    public class SyncServiceTests
    {
        private readonly SyncService _syncService;

        private readonly Mock<ITodoListService> _mockListService = new();
        private readonly Mock<ITodoItemService> _mockItemService = new();
        private readonly Mock<IExternalAPI> _mockExternalApi = new();
        private readonly Mock<ILogger<SyncService>> _mockLogger = new();

        private readonly IMapper _mapper;

        public SyncServiceTests()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TodoApi.Services.MappingProfile>();
            });

            _mapper = mapperConfig.CreateMapper();

            _syncService = new SyncService(
                _mockListService.Object,
                _mockItemService.Object,
                _mockExternalApi.Object,
                _mapper,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task SyncTodoListsAsync_ShouldReturnSyncResult_WhenEverythingWorks()
        {
            // Arrange
            _mockListService.Setup(s => s.GetTodoListsAsync(It.IsAny<bool>()))
                .ReturnsAsync(new List<TodoListDto>());

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList>());

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ItemCreations);
            Assert.Equal(0, result.ListCreations);
        }

        [Fact]
        public async Task SyncTodoListsAsync_ShouldCreateNewList_FromExternal_WhenNotExistsLocally()
        {
            // Arrange

            var localLists = new List<TodoListDto>();
            var externalLists = new List<ExternalTodoList>
            {
                new ExternalTodoList
                {
                    Id = "ext-1",
                    SourceId = null,
                    Name = "New External List",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    TodoItems = new List<ExternalTodoItem>()
                }
            };

            _mockListService
                .Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(localLists);

            _mockExternalApi
                .Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(externalLists);

            _mockListService
                .Setup(s => s.CreateTodoListAsync(It.IsAny<UpdateTodoListDto>()))
                .ReturnsAsync(new TodoListDto { Id = 1, Name = "New External List", ExternalId = "ext-1" });

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ListCreations);
            Assert.Equal(0, result.ListUpdates);
            Assert.Equal(0, result.ListDeleted);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WithNoLists_ShouldReturnZeroChanges()
        {
            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto>());

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList>());

            var result = await _syncService.SyncTodoListsAsync();

            Assert.NotNull(result);
            Assert.Equal(0, result.ListCreations);
            Assert.Equal(0, result.ListUpdates);
            Assert.Equal(0, result.ItemCreations);
            Assert.Equal(0, result.ItemUpdates);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenExternalListDoesNotExistLocally_ShouldCreateList()
        {
            var externalList = new ExternalTodoList
            {
                Id = Guid.NewGuid().ToString(),
                SourceId = "999",
                Name = "I'm a new External List",
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto>());

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockListService.Setup(s => s.CreateTodoListAsync(It.IsAny<UpdateTodoListDto>()))
                .ReturnsAsync(new TodoListDto { Id = 999, Name = externalList.Name });

            var result = await _syncService.SyncTodoListsAsync();

            Assert.NotNull(result);
            Assert.Equal(1, result.ListCreations);
            Assert.Equal(0, result.ListUpdates);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenExternalListNameChanged_ShouldUpdateLocalList()
        {
            var existingLocalList = new TodoListDto
            {
                Id = 1,
                Name = "Old name",
                ExternalId = "ext-1"
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-1",
                SourceId = "1",
                Name = "New name",
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { existingLocalList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockListService.Setup(s => s.UpdateTodoListAsync(It.IsAny<int>(), It.IsAny<UpdateTodoListDto>()))
                .ReturnsAsync(true);

            var result = await _syncService.SyncTodoListsAsync();

            Assert.NotNull(result);
            Assert.Equal(1, result.ListUpdates);
            Assert.Equal(0, result.ListCreations);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenExternalItemDoesNotExistLocally_ShouldCreateItem()
        {
            var externalItem = new ExternalTodoItem
            {
                Id = "ext-item-1",
                SourceId = "99", // No existe localmente
                Description = "New item",
                Completed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-list-1",
                SourceId = "1",
                Name = "External List",
                TodoItems = new List<ExternalTodoItem> { externalItem }
            };

            var existingLocalList = new TodoListDto
            {
                Id = 1,
                Name = "External List",
                ExternalId = "ext-list-1",
                Items = new List<TodoItemDto>() // Vacía
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { existingLocalList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockItemService.Setup(s => s.GetTodoItemsAsync(1))
                .ReturnsAsync(new List<TodoItemDto>());

            _mockItemService.Setup(s => s.CreateTodoItemAsync(It.IsAny<UpdateTodoItemDto>(), 1))
                .ReturnsAsync(new TodoItemDto { Id = 99 });

            var result = await _syncService.SyncTodoListsAsync();

            Assert.NotNull(result);
            Assert.Equal(1, result.ItemCreations);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenExternalItemChanged_ShouldUpdateLocalItem()
        {
            var externalItem = new ExternalTodoItem
            {
                Id = "ext-item-1",
                SourceId = "101",
                Description = "New desc",
                Completed = true
            };

            var localItem = new TodoItemDto
            {
                Id = 101,
                ExternalId = "ext-item-1",
                Description = "Old desc",
                Completed = false,
                Title = "Title"
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-list-1",
                SourceId = "1",
                Name = "List 1",
                TodoItems = new List<ExternalTodoItem> { externalItem }
            };

            var localList = new TodoListDto
            {
                Id = 1,
                Name = "Lista",
                ExternalId = "ext-list-1",
                Items = new List<TodoItemDto> { localItem }
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { localList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockItemService.Setup(s => s.GetTodoItemsAsync(1))
                .ReturnsAsync(new List<TodoItemDto> { localItem });

            _mockItemService.Setup(s => s.UpdateTodoItemAsync(It.IsAny<UpdateTodoItemDto>(), 1, 101))
                .ReturnsAsync(true);

            var result = await _syncService.SyncTodoListsAsync();

            Assert.NotNull(result);
            Assert.Equal(1, result.ItemUpdates);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenExternalItemUnchanged_ShouldDoNothing()
        {
            var externalItem = new ExternalTodoItem
            {
                Id = "ext-item-1",
                SourceId = "101",
                Description = "Item 1",
                Completed = true
            };

            var localItem = new TodoItemDto
            {
                Id = 101,
                ExternalId = "ext-item-1",
                Description = "Item 1",
                Completed = true,
                Title = "The Title"
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-list-1",
                SourceId = "1",
                Name = "List 1",
                TodoItems = new List<ExternalTodoItem> { externalItem }
            };

            var localList = new TodoListDto
            {
                Id = 1,
                Name = "List 1",
                ExternalId = "ext-list-1",
                Items = new List<TodoItemDto> { localItem }
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { localList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockItemService.Setup(s => s.GetTodoItemsAsync(1))
                .ReturnsAsync(new List<TodoItemDto> { localItem });

            var result = await _syncService.SyncTodoListsAsync();

            Assert.NotNull(result);
            Assert.Equal(0, result.ItemCreations);
            Assert.Equal(0, result.ItemUpdates);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenNewExternalListWithItem_ShouldCreateListAndItemLocally()
        {
            // Arrange: Lista e ítem externos nuevos
            var externalItem = new ExternalTodoItem
            {
                Id = "ext-item-1",
                SourceId = "999",
                Description = "A new external Item",
                Completed = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-list-1",
                SourceId = "10", // No existe localmente
                Name = "one new external list",
                TodoItems = new List<ExternalTodoItem> { externalItem }
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto>());

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            // Simula que al crear la lista, se devuelve con ID 10
            _mockListService.Setup(s => s.CreateTodoListAsync(It.IsAny<UpdateTodoListDto>()))
                .ReturnsAsync(new TodoListDto
                {
                    Id = 10,
                    Name = "one new external list",
                    ExternalId = "ext-list-1"
                });

            _mockItemService.Setup(s => s.GetTodoItemsAsync(10))
                .ReturnsAsync(new List<TodoItemDto>());

            _mockItemService.Setup(s => s.CreateTodoItemAsync(It.IsAny<UpdateTodoItemDto>(), 10))
                .ReturnsAsync(new TodoItemDto { Id = 999 });

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ListCreations);
            Assert.Equal(1, result.ItemCreations);
            Assert.Equal(0, result.ListUpdates);
            Assert.Equal(0, result.ItemUpdates);
        }


    }
}
