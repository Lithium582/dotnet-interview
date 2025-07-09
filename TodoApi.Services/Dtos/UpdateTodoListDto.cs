using System.ComponentModel.DataAnnotations;

namespace TodoApi.Services.Dtos;

public class UpdateTodoListDto
{
    [Required]
    public required string Name { get; set; }
}
