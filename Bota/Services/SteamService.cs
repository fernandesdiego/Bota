using Bota.Context;
using Bota.Models;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bota.Services
{
    public class SteamService
    {
        private readonly ApplicationContext _context;
        public SteamService(ApplicationContext contex)
        {
            _context = contex;
        }
        public async Task GetUserByName(SocketCommandContext commandContext, string userName)
        {
            await GetSteamUser(userName: userName, commandContext: commandContext);
        }

        public async Task GetSteamUser(SocketCommandContext commandContext, long steamId = 0, string userName = "")
        {
            var config = await _context.BotConfigs.FirstOrDefaultAsync();
            var steamApiKey = config.SteamApiKey;
            if (steamApiKey == string.Empty || steamApiKey == null)
            {
                await commandContext.Channel.SendMessageAsync("Configure a sua API key da Steam no painel primeiro");
            }

            if (steamId == 0 && userName == string.Empty)
            {
                await commandContext.Channel.SendMessageAsync("Preciso do nome de usuário ou Id.");
            }
            if (steamId == 0 && userName != string.Empty)
            {
                steamId = await GetSteamByName(userName, commandContext);
                if (steamId == 0)
                {
                    await commandContext.Channel.SendMessageAsync("Não encontrei esse usuário.");
                    return;
                }
            }

            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steamApiKey}&steamids={steamId}");

            JObject obj = JObject.Parse(result);
            JArray arr = JArray.Parse(obj["response"]["players"].ToString());

            if (arr.Count == 0)
            {
                var message = await commandContext.Channel.SendMessageAsync("Não encontrei esse usuário.");
                return;
            }

            SteamProfileInfo profile = JsonConvert.DeserializeObject<SteamProfileInfo>(arr[0].ToString());

            var gameCount = await client.GetStringAsync($"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={steamApiKey}&steamid={steamId}&format=json");
            JObject gameObj = JObject.Parse(gameCount);

            profile.GameCount = (int)gameObj["response"]["game_count"];

            var lvlBadge = await client.GetStringAsync($"http://api.steampowered.com/IPlayerService/GetBadges/v1/?key={steamApiKey}&steamid={steamId}");
            JObject badgeObj = JObject.Parse(lvlBadge);
            JArray badgeArr = JArray.Parse(badgeObj["response"]["badges"].ToString());

            profile.Level = (int)badgeObj["response"]["player_level"];
            profile.Xp = (int)badgeObj["response"]["player_xp"];
            profile.XpToNextLevel = (int)badgeObj["response"]["player_xp_needed_to_level_up"];
            profile.Badges = badgeArr.Count;

            var groupCount = await client.GetStringAsync($"http://api.steampowered.com/ISteamUser/GetUserGroupList/v0001/?key={steamApiKey}&steamid={steamId}");
            JObject groupObj = JObject.Parse(groupCount);
            JArray groupArr = JArray.Parse(groupObj["response"]["groups"].ToString());

            profile.GroupCount = groupArr.Count;

            CreateSteamBitmap(profile);

            var embed = new EmbedBuilder()
            {
                ImageUrl = "attachment://overlay.png"
            }
            .WithUrl(profile.ProfileUrl)
            .WithFooter(new EmbedFooterBuilder() { Text = profile.ProfileUrl, IconUrl = "https://e7.pngegg.com/pngimages/699/999/png-clipart-brand-logo-steam-gump-s.png" })
            .Build();

            await commandContext.Channel.SendFileAsync("overlay.png", embed: embed);

            //commandContext.Channel.SendMessageAsync(embed: embed);
        }
        private async Task<long> GetSteamByName([Remainder] string steamName, SocketCommandContext commandContext)
        {
            var config = await _context.BotConfigs.FirstOrDefaultAsync();
            var steamApiKey = config.SteamApiKey;
            if (steamApiKey == string.Empty || steamApiKey == null)
            {
                await commandContext.Channel.SendMessageAsync("Configure a sua API key da Steam no painel primeiro");
            }

            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={steamApiKey}&vanityurl={steamName}");
            JObject obj = JObject.Parse(result);
            long.TryParse((string)obj["response"]["steamid"], out long id);
            return id;
        }

        private void CreateSteamBitmap(SteamProfileInfo profile)
        {
            System.Drawing.Image background = System.Drawing.Image.FromFile("unknown.png"); // base img

            HttpWebRequest request = HttpWebRequest.Create(profile.AvatarFull) as HttpWebRequest;
            var response = request.GetResponse();
            Bitmap profilePic = new Bitmap(response.GetResponseStream());

            profilePic = new Bitmap(profilePic, 215, 215);

            Bitmap newImage = new Bitmap(background.Width, background.Height);

            using (Graphics gr = Graphics.FromImage(newImage))
            {
                gr.DrawImage(background, new Point(0, 0));

                gr.DrawString($"Nível {profile.Level}", new Font("Fira Code", 22f), Brushes.WhiteSmoke, 708, 42);
                gr.DrawString($"Insígnias {profile.Badges}", new Font("Fira Code", 22f), Brushes.WhiteSmoke, 708, 82);
                gr.DrawString($"Grupos {profile.GroupCount}", new Font("Fira Code", 22f), Brushes.WhiteSmoke, 708, 122);

                var nameLenght = profile.PersonaName.Length;

                gr.DrawString(profile.PersonaName.Length < 15 ? profile.PersonaName : profile.PersonaName.Substring(0, 15), new Font("Fira Code", 22f), Brushes.SlateGray, 326, 42);
                gr.DrawString($"{SteamStatus(profile.PersonaState)}", new Font("Fira Code", 18f), Brushes.WhiteSmoke, 326, 81);
                gr.DrawString($"Jogos {profile.GameCount}", new Font("Fira Code", 18f), Brushes.WhiteSmoke, 326, 173);

                float currentXp = profile.Xp;
                float toNextLvl = profile.XpToNextLevel;
                float xpPercent = currentXp / (currentXp + toNextLvl) * 100;

                float barWidth = 600;
                float currentProgress = xpPercent / 100 * barWidth;

                gr.DrawRectangle(new Pen(Brushes.DimGray), 326, 210, barWidth, 20);
                gr.FillRectangle(Brushes.DimGray, 326, 210, currentProgress, 20);

                gr.DrawString($"{currentXp} XP → {toNextLvl}", new Font("Fira Code", 10f), Brushes.WhiteSmoke, 326, 231);

                using (GraphicsPath gp = new GraphicsPath())
                {
                    var radius = 15 * 2;
                    var x = 57;
                    var y = 41;
                    var size = 215;
                    // create rounded square
                    gp.AddArc(x, y, radius, radius, 180, 90);
                    gp.AddArc(x + profilePic.Width - radius, 39, radius, radius, 270, 90);
                    gp.AddArc(x + profilePic.Width - radius, y + profilePic.Height - radius, radius, radius, 0, 90);
                    gp.AddArc(x, y + profilePic.Height - radius, radius, radius, 90, 90);

                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.SetClip(gp);
                    gr.DrawImage(profilePic, x, y, size, size);
                }
            }

            newImage.Save("overlay.png");
        }

        private static string SteamStatus(int number)
        {
            switch (number)
            {
                case 0:
                    return "Offline";
                case 1:
                    return "Online";
                case 2:
                    return "Ocupado";
                case 3:
                    return "Away";
                default:
                    return "Desconhecido";
            }
        }

    }
}
