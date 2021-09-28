using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bota.Models
{
    public class BotConfig
    {
        public ulong Id { get; set; }
        public string Prefix { get; set; }
        public string SteamApiKey { get; set; }
        public virtual IEnumerable<Message> Messages { get; set; }
    }
}