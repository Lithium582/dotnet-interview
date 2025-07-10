using AutoMapper;
using TodoApi.Data.Models;
using TodoApi.Services.Dtos;

namespace TodoApi.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TodoItem, TodoItemDto>()
                .ForMember(
                    dest => dest.ListName,
                    opt => opt.MapFrom(src => src.List != null ? src.List.Name : string.Empty)
                );
            CreateMap<UpdateTodoItemDto, TodoItem>();

            CreateMap<TodoItemDto, TodoItem>();

            CreateMap<TodoList, TodoListDto>();
            CreateMap<UpdateTodoListDto, TodoList>();

            CreateMap<TodoListDto, TodoList>();

            // Sync logic
            //CreateMap<ExternalTodoItem, TodoItem>();
            //CreateMap<TodoItem, ExternalTodoItem>();
        }
    }
}
