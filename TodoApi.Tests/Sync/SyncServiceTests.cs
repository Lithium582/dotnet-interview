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
        #region "Constructor and utils"
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

        #endregion

        #region "From External to Local"
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
                SourceId = "99",
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
                Items = new List<TodoItemDto>()
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
            // Arrange
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
                SourceId = "10",
                Name = "one new external list",
                TodoItems = new List<ExternalTodoItem> { externalItem }
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto>());

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

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

        [Fact]
        public async Task SyncTodoListsAsync_WhenCreateTodoListThrowsException_ShouldLogErrorAndContinue()
        {
            // Arrange
            var externalList = new ExternalTodoList
            {
                Id = "ext-1",
                Name = "Failed list",
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto>());

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockListService.Setup(s => s.CreateTodoListAsync(It.IsAny<UpdateTodoListDto>()))
                .ThrowsAsync(new Exception("Create failed"));

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ListCreations);
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Error syncing external list")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenUpdateTodoItemThrowsException_ShouldLogErrorAndContinue()
        {
            // Arrange
            var externalItem = new ExternalTodoItem
            {
                Id = "ext-item-1",
                SourceId = "101",
                Description = "external desc",
                Completed = false
            };

            var localItem = new TodoItemDto
            {
                Id = 101,
                ExternalId = "ext-item-1",
                Description = "old desc",
                Completed = false,
                Title = "Título"
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

            _mockItemService.Setup(s => s.UpdateTodoItemAsync(It.IsAny<UpdateTodoItemDto>(), 1, 101))
                .ThrowsAsync(new Exception("Item update failed"));

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ItemUpdates);
            _mockLogger.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Error syncing item")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenLocalListMatchesByExternalId_ShouldNotCreateDuplicate()
        {
            // Arrange
            var localList = new TodoListDto
            {
                Id = 1,
                Name = "Local list",
                ExternalId = "ext-123"
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-123",
                SourceId = null,
                Name = "Loca list updated",
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { localList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockListService.Setup(s => s.UpdateTodoListAsync(1, It.IsAny<UpdateTodoListDto>()))
                .ReturnsAsync(true);

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ListUpdates);
            Assert.Equal(0, result.ListCreations);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenExternalItemMatchesByExternalId_ShouldNotCreateDuplicate()
        {
            // Arrange
            var localItem = new TodoItemDto
            {
                Id = 1,
                ExternalId = "ext-item-1",
                Description = "Local desc",
                Completed = false,
                Title = "Title"
            };

            var externalItem = new ExternalTodoItem
            {
                Id = "ext-item-1",
                SourceId = null,
                Description = "External desc",
                Completed = false
            };

            var localList = new TodoListDto
            {
                Id = 1,
                Name = "List",
                ExternalId = "ext-list",
                Items = new List<TodoItemDto> { localItem }
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-list",
                SourceId = "1",
                Name = "List",
                TodoItems = new List<ExternalTodoItem> { externalItem }
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { localList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockItemService.Setup(s => s.GetTodoItemsAsync(1))
                .ReturnsAsync(new List<TodoItemDto> { localItem });

            _mockItemService.Setup(s => s.UpdateTodoItemAsync(It.IsAny<UpdateTodoItemDto>(), 1, 1))
                .ReturnsAsync(true);

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ItemUpdates);
            Assert.Equal(0, result.ItemCreations);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenListMissingOnExternal_ShouldSoftDeleteLocally()
        {
            // Arrange
            var localList = new TodoListDto
            {
                Id = 1,
                Name = "List deleted on external",
                ExternalId = "ext-list-1",
                Deleted = false
            };

            var localLists = new List<TodoListDto> { localList };
            var externalLists = new List<ExternalTodoList>();

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(localLists);

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(externalLists);

            _mockListService.Setup(s => s.DeleteTodoListAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ListDeleted);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenListDeletedLocally_ShouldDeleteOnExternal()
        {
            // Arrange
            var localList = new TodoListDto
            {
                Id = 1,
                Name = "Lista to delete on external",
                ExternalId = "ext-list-1",
                Deleted = true
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-list-1",
                SourceId = "1",
                Name = "Lista to delete on external",
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { localList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockExternalApi.Setup(e => e.DeleteTodoListAsync("ext-list-1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ListDeleted);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenItemMissingOnExternal_ShouldSoftDeleteLocally()
        {
            // Arrange
            var localItem = new TodoItemDto
            {
                Id = 1,
                ExternalId = "ext-item-1",
                Description = "Desc",
                Completed = false,
                Title = "Title",
                Deleted = false
            };

            var localList = new TodoListDto
            {
                Id = 1,
                Name = "List",
                ExternalId = "ext-list-1",
                Items = new List<TodoItemDto> { localItem }
            };

            var externalList = new ExternalTodoList
            {
                Id = "ext-list-1",
                SourceId = "1",
                Name = "List",
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto> { localList });

            _mockExternalApi.Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockItemService.Setup(s => s.GetTodoItemsAsync(1))
                .ReturnsAsync(new List<TodoItemDto> { localItem });

            _mockItemService.Setup(s => s.DeleteTodoItemAsync(1, 1))
                .ReturnsAsync(true);

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ItemDeleted);
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenCreateListFails_ShouldLogErrorAndContinue()
        {
            // Arrange
            var externalList = new ExternalTodoList
            {
                Id = "ext-1",
                SourceId = null,
                Name = "New list",
                TodoItems = new List<ExternalTodoItem>()
            };

            _mockListService.Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(new List<TodoListDto>());

            _mockExternalApi.Setup(s => s.GetTodoListsAsync())
                .ReturnsAsync(new List<ExternalTodoList> { externalList });

            _mockListService.Setup(s => s.CreateTodoListAsync(It.IsAny<UpdateTodoListDto>()))
                .ThrowsAsync(new Exception("Simulated failure when creating a list"));

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.ListCreations);
            Assert.Equal(0, result.ItemCreations);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error syncing external list")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task SyncTodoListsAsync_WhenItemUpdateFails_ShouldLogErrorAndContinue()
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
                Name = "List",
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
                .ThrowsAsync(new Exception("Simulated failure when creating an item"));

            // Act
            var result = await _syncService.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ItemUpdates);

            _mockLogger.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error syncing item")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        #endregion
    }
}
