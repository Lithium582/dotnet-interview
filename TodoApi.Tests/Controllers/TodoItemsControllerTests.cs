using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Controllers;
using TodoApi.Services.Dtos;
using TodoApi.Services.Services;

namespace TodoApi.Tests.Controllers;

#nullable disable
public class TodoItemsControllerTests
{
    #region "Constructor and utils"
    private readonly Mock<ITodoItemService> _mockService;
    private readonly TodoItemController _controller;

    public TodoItemsControllerTests()
    {
        _mockService = new Mock<ITodoItemService>();
        _controller = new TodoItemController(_mockService.Object);
    }

    private List<TodoItemDto> GetFakeTodoItems()
    {
        return new List<TodoItemDto>
        {
            new TodoItemDto { Id = 1, Title = "Item 1", ListName = "Lista 1", Completed = false, Description = "Item 1" },
            new TodoItemDto { Id = 2, Title = "Item 2", ListName = "Lista 2", Completed = false, Description = "Item 2" },
            new TodoItemDto { Id = 3, Title = "Item 3", ListName = "Lista 2", Completed = true, Description = "Item 3" },
        };
    }
    #endregion

    #region "Gets"
    [Fact]
    public async Task GetTodoItems_WhenCalled_ReturnsTodoItemsList()
    {
        // Arrange
        var expected = GetFakeTodoItems();

        _mockService
            .Setup(s => s.GetTodoItemsAsync(1))
            .ReturnsAsync(expected.Where(i => i.ListName == "Lista 1").ToList());

        // Act
        var result = await _controller.GetTodoItems(1);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        List<TodoItemDto> returnValue = Assert.IsType<List<TodoItemDto>>(okResult.Value);

        Assert.Equal("Item 1", returnValue[0].Title);
        Assert.Single(returnValue);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public async Task GetTodoItems_WhenCalled_ReturnsTodoItemsById(long listId, long itemId)
    {
        // Arrange
        var expected = GetFakeTodoItems();

        _mockService
            .Setup(s => s.GetTodoItemAsync(listId, itemId))
            .ReturnsAsync(expected.FirstOrDefault(i => i.Id == itemId));

        // Act
        var result = await _controller.GetTodoItem(listId, itemId);
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        TodoItemDto dto = Assert.IsType<TodoItemDto>(okResult.Value);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(itemId, dto.Id);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(2, 20)]
    public async Task GetTodoList_WhenCalled_ReturnsNullWhenNotExists(long listId, long itemId)
    {
        // Arrange
        var expected = GetFakeTodoItems();

        _mockService
            .Setup(s => s.GetTodoItemAsync(listId, itemId))
            .ReturnsAsync(expected.FirstOrDefault(i => i.Id == itemId));

        // Act
        var result = await _controller.GetTodoItem(listId, itemId);
        NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    #endregion

    #region "Puts"
    [Theory]
    [InlineData(1,1)]
    [InlineData(2,3)]
    public async Task PutTodoItem_WhenTodoListDoesntExist_ReturnsNotFound(long listId, long itemId)
    {
        // Arrange
        var expected = GetFakeTodoItems();
        UpdateTodoItemDto itemToPut = new UpdateTodoItemDto { Title = "Item 35", ListId = listId, Description = "Item 3", Completed = false };

        _mockService
            .Setup(s => s.UpdateTodoItemAsync(itemToPut, listId, itemId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.PutTodoItem(itemToPut, listId, itemId);
        NotFoundResult notFoundResult = Assert.IsType<NotFoundResult>(result.Result);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    public async Task PutTodoItem_WhenCalled_UpdatesTheTodoItem(long listId, long itemId)
    {
        // Arrange
        var expected = GetFakeTodoItems();
        UpdateTodoItemDto itemToPut = new UpdateTodoItemDto { Title = "Item 35", ListId = listId, Description = "Item 3", Completed = false };

        _mockService
            .Setup(s => s.UpdateTodoItemAsync(itemToPut, listId, itemId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.PutTodoItem(itemToPut, listId, itemId);
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result.Result);
        bool dto = Assert.IsType<bool>(okResult.Value);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        Assert.True(dto);
    }
    #endregion

    #region "Posts"

    [Fact]
    public async Task PostTodoItem_WhenCalled_CreatesTodoItem()
    {
        // Arrange
        TodoItemDto expected = new TodoItemDto
        {
            Id = 4,
            Title = "Item 1",
            ListName = "Lista 1",
            Completed = false,
            Description = "Item 1"
        };

        UpdateTodoItemDto itemToPost = new UpdateTodoItemDto
        {
            Title = "Item 4",
            ListId = 1,
            Completed = false,
            Description = "Item 4"
        };

        _mockService
            .Setup(s => s.CreateTodoItemAsync(itemToPost, 1))
            .ReturnsAsync(expected);

        // Act
        var result = await _controller.CreateTodoItemAsync(itemToPost, 1);
        CreatedAtActionResult createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        TodoItemDto dto = Assert.IsType<TodoItemDto>(createdAtActionResult.Value);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(4, dto.Id);
    }

    #endregion

    #region "Deletes"
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    public async Task DeleteTodoItem_WhenCalled_RemovesTodoItem(long listId, long itemId)
    {
        // Arrange

        _mockService
            .Setup(s => s.DeleteTodoItemAsync(listId, itemId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTodoList(listId, itemId);

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(2, 6)]
    public async Task DeleteTodoItem_WhenCalled_CantRemoveTodoItem(long listId, long itemId)
    {
        // Arrange

        _mockService
            .Setup(s => s.DeleteTodoItemAsync(listId, itemId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTodoList(listId, itemId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    #endregion
}
