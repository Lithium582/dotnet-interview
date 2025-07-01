using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoApi.Services.Dtos
{
    public class CreateTodoItem
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
    }
}
