using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedModels
{
    public class Message
    {
        public string Action { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}
