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
        [Fact]
        public async Task SyncTodoListsAsync_Should_Create_List_From_External_When_Not_Exists_Locally()
        {
            // Arrange
            var mockListService = new Mock<ITodoListService>();
            var mockItemService = new Mock<ITodoItemService>();
            var mockExternalApi = new Mock<IExternalAPI>();
            var mockMapper = new Mock<IMapper>();
            var mockLogger = new Mock<ILogger<SyncService>>();

            var localLists = new List<TodoListDto>(); // Local vacío
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

            mockListService
                .Setup(s => s.GetTodoListsAsync(true))
                .ReturnsAsync(localLists);

            mockExternalApi
                .Setup(e => e.GetTodoListsAsync())
                .ReturnsAsync(externalLists);

            mockMapper
                .Setup(m => m.Map<UpdateTodoListDto>(It.IsAny<ExternalTodoList>()))
                .Returns(new UpdateTodoListDto { Name = "New External List" });

            mockListService
                .Setup(s => s.CreateTodoListAsync(It.IsAny<UpdateTodoListDto>()))
                .ReturnsAsync(new TodoListDto { Id = 1, Name = "New External List" });

            var service = new SyncService(
                mockListService.Object,
                mockItemService.Object,
                mockExternalApi.Object,
                mockMapper.Object,
                mockLogger.Object
            );

            // Act
            var result = await service.SyncTodoListsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.ListCreations);
            Assert.Equal(0, result.ListUpdates);
            Assert.Equal(0, result.ListDeleted);
        }
    }
}
