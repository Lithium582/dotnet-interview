using AutoMapper;
using TodoApi.Data.Models;
using TodoApi.ExternalContracts.Contracts;
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
                )
                .ForMember(
                    dest => dest.ListId,
                    opt => opt.MapFrom(src => src.List != null ? src.List.Id : 0)
                );

            CreateMap<UpdateTodoItemDto, TodoItem>();

            CreateMap<TodoItemDto, TodoItem>();

            CreateMap<TodoList, TodoListDto>();
            CreateMap<UpdateTodoListDto, TodoList>();

            CreateMap<TodoListDto, TodoList>();

            // Sync logic
            CreateMap<ExternalTodoItem, UpdateTodoItemDto>()
                .ForMember(dest => dest.Title, opt => opt.Ignore()) // si no existe en la API externa
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Completed, opt => opt.MapFrom(src => src.Completed));
        }
    }
}
