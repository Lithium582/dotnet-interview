using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using TodoApi.Controllers;
using TodoApi.Data.Models;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.Tests.Controllers;

#nullable disable
public class TodoListsControllerTests
{
    private readonly Mock<ITodoListService> _mockService;
    private readonly TodoListsController _controller;

    public TodoListsControllerTests()
    {
        _mockService = new Mock<ITodoListService>();
        _controller = new TodoListsController(_mockService.Object);
    }

    private List<TodoListDto> AssertMockService()
    {
        return new List<TodoListDto>
        {
            new TodoListDto { Id = 1, Name = "Test List 1", Items = new() },
            new TodoListDto { Id = 2, Name = "Test List 2", Items = new() }
        };
    }

    [Fact]
    public async Task GetTodoList_WhenCalled_ReturnsTodoListList()
    {
        // Arrange
        var expected = AssertMockService();

        _mockService
            .Setup(s => s.GetTodoListsAsync())
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetTodoLists();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<TodoListDto>>(okResult.Value);

        Assert.Equal("Test List 1", returnValue[0].Name);
        Assert.Equal(2, returnValue.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetTodoList_WhenCalled_ReturnsTodoListById(long listId)
    {
        // Arrange
        var expected = AssertMockService();

        _mockService
            .Setup(s => s.GetTodoListAsync(listId))
            .ReturnsAsync(expected.FirstOrDefault(l => l.Id == listId));

        // Act
        var result = await _controller.GetTodoList(listId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(listId, ((result.Result as OkObjectResult).Value as TodoListDto).Id);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    public async Task GetTodoList_WhenCalled_ReturnsNullWhenNotExists(long listId)
    {
        // Arrange
        var expected = AssertMockService();

        _mockService
            .Setup(s => s.GetTodoListAsync(listId))
            .ReturnsAsync(expected.FirstOrDefault(l => l.Id == listId));

        // Act
        var result = await _controller.GetTodoList(1);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, (result.Result as NotFoundResult).StatusCode);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public async Task PutTodoList_WhenTodoListDoesntExist_ReturnsBadRequest(long listId)
    {
        // Arrange
        var expected = AssertMockService();
        UpdateTodoListDto listToPut = new UpdateTodoListDto { Name = "lista 3" };

        _mockService
            .Setup(s => s.UpdateTodoListAsync(listId, listToPut))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.PutTodoList(listId, listToPut);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, (result.Result as NotFoundResult).StatusCode);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public async Task PutTodoList_WhenCalled_UpdatesTheTodoList(long listId)
    {
        // Arrange
        UpdateTodoListDto listToPut = new UpdateTodoListDto { Name = "lista 3" };

        _mockService
            .Setup(s => s.UpdateTodoListAsync(listId, listToPut))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.PutTodoList(listId, listToPut);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.True((bool)(result.Result as OkObjectResult).Value);
    }

    [Fact]
    public async Task PostTodoList_WhenCalled_CreatesTodoList()
    {
        // Arrange
        TodoListDto expected = new TodoListDto
        {
            Name = "lista 3",
            Id = 3,
            Items = null
        };

        UpdateTodoListDto listToPost = new UpdateTodoListDto { Name = "lista 3" };

        _mockService
            .Setup(s => s.CreateTodoListAsync(listToPost))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.PostTodoList(listToPost);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(3, ((result.Result as CreatedAtActionResult).Value as TodoListDto).Id);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task DeleteTodoList_WhenCalled_RemovesTodoList(long listId)
    {
        // Arrange

        _mockService
            .Setup(s => s.DeleteTodoListAsync(listId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTodoList(listId);

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public async Task DeleteTodoList_WhenCalled_CantRemoveTodoList(long listId)
    {
        // Arrange

        _mockService
            .Setup(s => s.DeleteTodoListAsync(listId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTodoList(listId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
