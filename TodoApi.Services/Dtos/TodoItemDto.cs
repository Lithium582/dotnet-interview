using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoApi.Data.Models;

namespace TodoApi.Services.Dtos
{
    public class TodoItemDto
    {
        public string ListName { get; set; }
        public long ListId { get; set; }
        public long Id { get; set; }
        public bool Completed { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ExternalId { get; set; }
        public bool Deleted { get; set; }
    }
}
