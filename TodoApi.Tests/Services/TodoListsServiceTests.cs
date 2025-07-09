using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TodoApi.Controllers;
using TodoApi.Data.Models;
using TodoApi.Services;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.Tests.Services;

public class TodoListsServiceTests
{
    #region "Constructor and utils"
    private readonly IMapper _mapper;
    private readonly TodoListService _service;

    public TodoListsServiceTests()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = configuration.CreateMapper();
        var context = new TodoContext(DatabaseContextOptions());
        _service = new TodoListService(context, _mapper);
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
        context.TodoList.Add(new TodoList { Id = 1, Name = "Task 1" });
        context.TodoList.Add(new TodoList { Id = 2, Name = "Task 2" });
        context.SaveChanges();
    }
    #endregion

    #region "Gets"
    [Fact]
    public async Task GetTodoListsAsync_ReturnsAllLists()
    {
        // Act
        var result = await _service.GetTodoListsAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Task 1", result[0].Name);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetTodoListAsync_ReturnsOneList(long listId)
    {
        // Act
        var result = await _service.GetTodoListAsync(listId);

        // Assert
        Assert.Equal(listId, result.Id);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public async Task GetTodoListAsync_ReturnsNull_WhenListDoesNotExist(long listId)
    {
        // Act
        var result = await _service.GetTodoListAsync(listId);

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region "Puts"
    [Fact]
    public async Task UpdateTodoListAsync_ReturnsFalse_WhenListDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateTodoListDto { Name = "Task 1 bis" };

        // Act
        var result = await _service.UpdateTodoListAsync(3, updateDto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTodoListAsync_ReturnsTrue_WhenListExists()
    {
        // Arrange
        var updateDto = new UpdateTodoListDto { Name = "Task 2 bis" };

        // Act
        var result = await _service.UpdateTodoListAsync(2, updateDto);

        // Assert
        Assert.True(result);

        var updated = await _service.GetTodoListAsync(2);
        Assert.Equal("Task 2 bis", updated.Name);
    }

    #endregion

    #region "Posts"
    [Fact]
    public async Task CreateTodoListAsync_ReturnsTodoList_WhenListCreated()
    {
        // Arrange
        var updateDto = new UpdateTodoListDto { Name = "Task 3" };

        // Act
        var result = await _service.CreateTodoListAsync(updateDto);

        // Assert
        Assert.Equal(3, result.Id);
    }

    [Fact]
    public async Task CreateTodoListAsync_ReturnsException_WhenDtoInvalid()
    {
        // Arrange
        var createDto = new UpdateTodoListDto { Name = null };

        // Act // Assert
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            _service.CreateTodoListAsync(createDto));
    }

    #endregion

    #region "Deletes"

    [Fact]
    public async Task DeleteTodoListAsync_ReturnsTrue_WhenDeleteSuccess()
    {
        // Act
        var result = await _service.DeleteTodoListAsync(2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteTodoListAsync_ReturnsFalse_WhenDeleteFails()
    {
        // Act
        var result = await _service.DeleteTodoListAsync(5);

        // Assert
        Assert.False(result);
    }


    #endregion
}
