using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}