namespace Bota.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using ImageMagick;

    public class SteamProfileInfo
    {
        public string Steamid { get; set; }
        public string PersonaName { get; set; }
        public string ProfileUrl { get; set; }
        public string AvatarFull { get; set; }
        public int PersonaState { get; set; }
        public int GameCount { get; set; }
        public int GroupCount { get; set; }
        public int Badges { get; set; }
        public float Xp { get; set; }
        public int Level { get; set; }
        public float XpToNextLevel { get; set; }
        public MagickImage ProfilePic
        {
            get
            {
                HttpWebRequest request = HttpWebRequest.Create(AvatarFull) as HttpWebRequest;
                var response = request.GetResponse();
                return new MagickImage(response.GetResponseStream());
            }
        }
    }
}
