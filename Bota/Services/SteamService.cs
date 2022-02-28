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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
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

        public async Task GetSteamUser(int id3, SocketCommandContext commandContext)
        {
            var id64 = GetSteamId64(id3);

            var apiKey = await GetSteamApiKey();
            if (string.IsNullOrEmpty(apiKey))
            {
                _ = commandContext.Channel.SendMessageAsync("Não encontrei uma chave de api steam válida, verifique o painel");
                return;
            }

            var steamProfile = await GetSteamProfile(id64, apiKey);

            if (steamProfile == null)
            {
                _ = commandContext.Channel.SendMessageAsync("Não encontrei esse usuário; Verifique o número informado e se o perfil da steam está público.");
                return;
            }

            steamProfile.GameCount = await GetSteamGames(id64, apiKey);

            steamProfile.Badges = await GetSteamBadges(id64, apiKey);

            var playerXp = await GetPlayerXp(id64, apiKey);

            steamProfile.Xp = (int)playerXp["response"]["player_xp"];
            steamProfile.XpToNextLevel = (int)playerXp["response"]["player_xp_needed_to_level_up"];
            steamProfile.Level = (int)playerXp["response"]["player_level"];

            steamProfile.GroupCount = await GetPlayerGroups(id64, apiKey);

            CreateSteamBitmap(steamProfile);

            var embed = new EmbedBuilder()
            {
                ImageUrl = "attachment://overlay.png",
            }
            .WithUrl(steamProfile.ProfileUrl)
            .WithFooter(new EmbedFooterBuilder() { Text = $"{steamProfile.ProfileUrl} \n{id3}", IconUrl = "https://e7.pngegg.com/pngimages/699/999/png-clipart-brand-logo-steam-gump-s.png" })
            .Build();

            await commandContext.Message.DeleteAsync();
            await commandContext.Channel.SendFileAsync($"{AppContext.BaseDirectory}overlay.png", embed: embed);
        }

        public async Task<string> GetSteamApiKey()
        {
            var config = await _context.BotConfigs.FirstOrDefaultAsync();
            return config.SteamApiKey;
        }

        public async Task<int> GetPlayerGroups(long id64, string apiKey)
        {
            var client = new HttpClient();
            var reqResult = await client.GetStringAsync($"http://api.steampowered.com/ISteamUser/GetUserGroupList/v0001/?key={apiKey}&steamid={id64}");
            var reqObj = JObject.Parse(reqResult);
            return JArray.Parse(reqObj["response"]["groups"].ToString()).Count;
        }

        public async Task<JObject> GetPlayerXp(long id64, string apiKey)
        {
            var client = new HttpClient();
            var reqResult = await client.GetStringAsync($"http://api.steampowered.com/IPlayerService/GetBadges/v1/?key={apiKey}&steamid={id64}");
            var reqObj = JObject.Parse(reqResult);
            //return JArray.Parse(reqObj["response"].ToString());
            return reqObj;
        }

        public async Task<int> GetSteamBadges(long id64, string apiKey)
        {
            var client = new HttpClient();
            var reqResult = await client.GetStringAsync($"http://api.steampowered.com/IPlayerService/GetBadges/v1/?key={apiKey}&steamid={id64}");
            var reqObj = JObject.Parse(reqResult);
            int badgeCount = 0;
            try
            {
                var badgeArr = JArray.Parse(reqObj["response"]["badges"].ToString());
                badgeCount = badgeArr.Count;
            }
            catch (Exception e)
            {
                badgeCount = 0;
            }

            return badgeCount;
        }

        public async Task<int> GetSteamGames(long id64, string apiKey)
        {
            var client = new HttpClient();
            var reqResult = await client.GetStringAsync($"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={apiKey}&steamid={id64}&format=json");

            var gameObj = JObject.Parse(reqResult);
            int gameCount = 0;
            try
            {
                gameCount = (int)gameObj["response"]["game_count"];
            }
            catch (Exception e)
            {
                gameCount = 0;
            }
            return gameCount;
        }

        public async Task<SteamProfileInfo> GetSteamProfile(long id64, string apiKey)
        {
            var client = new HttpClient();
            var reqResult = await client.GetAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={apiKey}&steamids={id64}");

            if (reqResult.IsSuccessStatusCode)
            {
                if (reqResult.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }

                var reqObj = JObject.Parse(await reqResult.Content.ReadAsStringAsync());
                var reqArr = JArray.Parse(reqObj["response"]["players"].ToString());

                if (reqArr.Count != 0)
                {
                    return JsonConvert.DeserializeObject<SteamProfileInfo>(reqArr[0].ToString());
                }
            }

            return null;
        }

        private void CreateSteamBitmap(SteamProfileInfo profile)
        {
            var currentPath = Directory.GetCurrentDirectory();
            Console.Write(Path.Combine(currentPath, "unknown.png"));
            System.Drawing.Image background = System.Drawing.Image.FromFile(Path.Combine(AppContext.BaseDirectory, "unknown.png")); // base img

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

            newImage.Save(Path.Combine(AppContext.BaseDirectory, "overlay.png"));
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

        private Int64 GetSteamId64(int id3)
        {
            const Int64 steamIdentifier = 76561197960265728;

            return steamIdentifier + id3;
        }
    }
}
