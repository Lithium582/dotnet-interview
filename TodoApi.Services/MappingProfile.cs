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
            CreateMap<UpdateTodoItemDto, TodoItemDto>();
            CreateMap<TodoItemDto, UpdateTodoItemDto>();

            CreateMap<TodoItemDto, TodoItem>();

            CreateMap<TodoList, TodoListDto>();
            CreateMap<UpdateTodoListDto, TodoList>();
            CreateMap<TodoListDto, UpdateTodoListDto>();
            CreateMap<UpdateTodoListDto, TodoListDto>();

            CreateMap<TodoListDto, TodoList>();

            // Sync logic
            CreateMap<ExternalTodoItem, UpdateTodoItemDto>()
                .ForMember(dest => dest.Title,
                opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.ExternalId,
                opt => opt.MapFrom(src => src.Id));

            CreateMap<UpdateTodoItemDto, ExternalTodoItem>()
                .ForMember(dest => dest.SourceId,
                opt => opt.MapFrom(src => src.ExternalId));
            CreateMap<UpdateExternalTodoItem, ExternalTodoItem>();
            CreateMap<ExternalTodoItem, UpdateExternalTodoItem>();
            CreateMap<TodoItemDto, UpdateExternalTodoItem>();
            CreateMap<UpdateExternalTodoItem, TodoItemDto>();

            CreateMap<UpdateTodoListDto, ExternalTodoList>()
                .ForMember(dest => dest.SourceId,
                opt => opt.MapFrom(src => src.ExternalId));
            CreateMap<UpdateExternalTodoList, ExternalTodoList>();
            CreateMap<ExternalTodoList, UpdateExternalTodoList>();
            CreateMap<TodoListDto, CreateExternalTodoList>()
                .ForMember(dest => dest.SourceId,
                opt => opt.MapFrom(src => src.Id.ToString()));

            CreateMap<CreateExternalTodoList, TodoListDto>()
                .ForMember(dest => dest.ExternalId,
                opt => opt.MapFrom(src => src.SourceId));
            CreateMap<ExternalTodoList, UpdateTodoListDto>()
                .ForMember(dest => dest.ExternalId,
                opt => opt.MapFrom(src => src.Id));
        }
    }
}
