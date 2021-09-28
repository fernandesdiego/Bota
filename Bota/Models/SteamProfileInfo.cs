namespace Bota.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class SteamProfileInfo
    {
        public string Steamid { get; set; }
        public string PersonaName { get; set; }
        public string ProfileUrl { get; set; }
        public string AvatarFull { get; set; }
        public long LastLogoff { get; set; }
        public int PersonaState { get; set; }
        public string RealName { get; set; }

    }
}
