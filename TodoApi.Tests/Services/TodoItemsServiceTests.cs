using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data.Models;
using TodoApi.Services;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.Tests.Services;

public class TodoItemsServiceTests
{
    #region "Constructor and utils"
    private readonly IMapper _mapper;
    private readonly TodoItemService _service;

    public TodoItemsServiceTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = configuration.CreateMapper();
        var context = new TodoContext(DatabaseContextOptions());
        _service = new TodoItemService(context, _mapper);
        PopulateDatabaseContext(context);
    }

    private DbContextOptions<TodoContext> DatabaseContextOptions()
    {
        return new DbContextOptionsBuilder<TodoContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private void PopulateDatabaseContext(TodoContext context)
    {
        context.TodoList.Add(new TodoList { Id = 1, Name = "List 1" });
        context.TodoList.Add(new TodoList { Id = 2, Name = "List 2" });

        context.TodoItem.Add(new TodoItem { Id = 1, Description = "Task 1", Title = "Task 1", Completed = true, ListId = 1 });
        context.TodoItem.Add(new TodoItem { Id = 2, Description = "Task 2", Title = "Task 2", Completed = true, ListId = 1 });
        context.TodoItem.Add(new TodoItem { Id = 3, Description = "Task 3", Title = "Task 3", Completed = true, ListId = 2 });
        context.SaveChanges();
    }
    #endregion

    #region "Gets"
    [Fact]
    public async Task GetTodoItemsAsync_ReturnsAllItems_ForAList()
    {
        // Act
        var result = await _service.GetTodoItemsAsync(1);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Task 1", result[0].Title);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    public async Task GetTodoItemAsync_ReturnsOneItem(long listId, long itemId)
    {
        // Act
        var result = await _service.GetTodoItemAsync(listId, itemId);

        // Assert
        Assert.Equal(itemId, result.Id);
    }

    [Theory]
    [InlineData(3, 4)]
    [InlineData(4, 5)]
    public async Task GetTodoItemAsync_ReturnsNull_WhenListOrItemNotExist(long listId, long itemId)
    {
        // Act
        var result = await _service.GetTodoItemAsync(listId, itemId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTodoItemsAsync_ReturnsEmptyList_WhenListDoesNotExist()
    {
        // Act
        var result = await _service.GetTodoItemsAsync(4);

        // Assert
        Assert.Empty(result);
    }

    #endregion

    #region "Puts"
    [Fact]
    public async Task UpdateTodoItemAsync_ReturnsFalse_WhenItemDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateTodoItemDto { Title = "Updated Item", Description = "Updated Item", Completed = false };

        // Act
        var result = await _service.UpdateTodoItemAsync(updateDto, 4, 5);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTodoItemAsync_ReturnsTrue_WhenItemExists()
    {
        // Arrange
        var updateDto = new UpdateTodoItemDto { Title = "Updated Item", Description = "Updated Item", Completed = false };

        // Act
        var result = await _service.UpdateTodoItemAsync(updateDto, 1, 1);

        // Assert
        Assert.True(result);

        var updated = await _service.GetTodoItemAsync(1, 1);
        Assert.Equal("Updated Item", updated.Title);
    }

    #endregion

    #region "Posts"
    [Fact]
    public async Task CreateTodoItemAsync_ReturnsTodoItem_WhenItemCreated()
    {
        // Arrange
        var createDto = new UpdateTodoItemDto { Title = "New Item", Description = "New Item", Completed = false };

        // Act
        var result = await _service.CreateTodoItemAsync(createDto, 1);

        // Assert
        Assert.Equal(4, result.Id);
    }

    [Fact]
    public async Task CreateTodoItemAsync_ReturnsException_WhenDtoInvalid()
    {
        // Arrange
        var createDto = new UpdateTodoItemDto { Title = null, Description = null, Completed = false };

        // Act // Assert
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _service.CreateTodoItemAsync(createDto, 1));
    }

    [Fact]
    public async Task CreateTodoItemAsync_ReturnsListNull_WhenListDoesNotExist()
    {
        // Arrange
        var createDto = new UpdateTodoItemDto { Title = "New Item", Description = "New Item", Completed = false };

        // Act
        var result = await _service.CreateTodoItemAsync(createDto, 3);

        // Assert
        Assert.Empty(result.ListName);
    }

    #endregion

    #region "Deletes"

    [Fact]
    public async Task DeleteTodoItemAsync_ReturnsTrue_WhenDeleteSuccess()
    {
        // Act
        var result = await _service.DeleteTodoItemAsync(1,1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteTodoItemAsync_ReturnsFalse_WhenDeleteFails()
    {
        // Act
        var result = await _service.DeleteTodoItemAsync(5,5);

        // Assert
        Assert.False(result);
    }


    #endregion
}
