﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TodoApi.Data.Models;
using TodoApi.Services.Dtos;

namespace TodoApi.Services.Services
{
    public class TodoListService : ITodoListService
    {
        private readonly TodoContext _context;
        private readonly IMapper _mapper;

        public TodoListService(TodoContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IList<TodoListDto>> GetTodoListsAsync()
        {
            var todoLists =
                await _context.TodoList
                .Include(l => l.Items)
                .ToListAsync();

            return _mapper.Map<IList<TodoListDto>>(todoLists);
        }

        public async Task<TodoListDto> GetTodoListAsync(long listId)
        {
            var todoList = await _context.TodoList
                .Include((l) => l.Items)
                .Where((l) => l.Id == listId)
                .FirstOrDefaultAsync();

            return _mapper.Map<TodoListDto>(todoList);
        }

        public async Task<TodoListDto> CreateTodoListAsync(UpdateTodoListDto todoListDto)
        {
            var todoList = _mapper.Map<TodoList>(todoListDto);

            _context.TodoList.Add(todoList);
            await _context.SaveChangesAsync();

            return _mapper.Map<TodoListDto>(todoList);
        }

        public async Task<bool> UpdateTodoListAsync(long listId, UpdateTodoListDto todoListDto)
        {
            var todoList = await _context.TodoList.FirstAsync((l) => l.Id == listId);

            if (todoList != null)
            {
                todoList.Name = todoListDto.Name;

                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<bool> DeleteTodoListAsync(long listId)
        {
            var todoList = await _context.TodoList.FirstAsync((item) => item.Id == listId);

            if (todoList != null)
            {
                _context.TodoList.Remove(todoList);
                await _context.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
