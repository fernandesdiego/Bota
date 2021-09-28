using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bota.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string MessageText { get; set; }
        public MessageType MessageType { get; set; }
        public ulong BotConfigId { get; set; }
    }
}
