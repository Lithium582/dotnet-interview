using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Controllers;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.Tests.Controllers;

#nullable disable
public class TodoListsControllerTests
{
    #region "Constructor and utils"
    private readonly Mock<ITodoListService> _mockService;
    private readonly TodoListsController _controller;

    public TodoListsControllerTests()
    {
        _mockService = new Mock<ITodoListService>();
        _controller = new TodoListsController(_mockService.Object);
    }

    private List<TodoListDto> GetFakeTodoLists()
    {
        return new List<TodoListDto>
        {
            new TodoListDto { Id = 1, Name = "Test List 1", Items = new() },
            new TodoListDto { Id = 2, Name = "Test List 2", Items = new() }
        };
    }

    #endregion

    #region "Gets"
    [Fact]
    public async Task GetTodoList_WhenCalled_ReturnsTodoListList()
    {
        // Arrange
        var expected = GetFakeTodoLists();

        _mockService
            .Setup(s => s.GetTodoListsAsync(false))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.GetTodoLists();

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        List<TodoListDto> returnValue = Assert.IsType<List<TodoListDto>>(okResult.Value);

        Assert.Equal("Test List 1", returnValue[0].Name);
        Assert.Equal(2, returnValue.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public async Task GetTodoList_WhenCalled_ReturnsTodoListById(long listId)
    {
        // Arrange
        var expected = GetFakeTodoLists();

        _mockService
            .Setup(s => s.GetTodoListAsync(listId))
            .ReturnsAsync(expected.FirstOrDefault(l => l.Id == listId));

        // Act
        var result = await _controller.GetTodoList(listId);
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        TodoListDto dto = Assert.IsType<TodoListDto>(okResult.Value);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(listId, dto.Id);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(20)]
    public async Task GetTodoList_WhenCalled_ReturnsNullWhenNotExists(long listId)
    {
        // Arrange
        var expected = GetFakeTodoLists();

        _mockService
            .Setup(s => s.GetTodoListAsync(listId))
            .ReturnsAsync(expected.FirstOrDefault(l => l.Id == listId));

        // Act
        var result = await _controller.GetTodoList(listId);
        NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region "Puts"
    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    public async Task PutTodoList_WhenTodoListDoesntExist_ReturnsNotFound(long listId)
    {
        // Arrange
        var expected = GetFakeTodoLists();
        UpdateTodoListDto listToPut = new UpdateTodoListDto { Name = "lista 3" };

        _mockService
            .Setup(s => s.UpdateTodoListAsync(listId, listToPut))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.PutTodoList(listId, listToPut);
        NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
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
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        bool dto = Assert.IsType<bool>(okResult.Value);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.True(dto);
    }
    #endregion

    #region "Posts"

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
        CreatedAtActionResult createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        TodoListDto dto = Assert.IsType<TodoListDto>(createdAtActionResult.Value);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(3, dto.Id);
    }

    #endregion

    #region "Deletes"
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

    #endregion
}
