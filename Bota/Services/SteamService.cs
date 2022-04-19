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
using ImageMagick;

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
            int x = 57;
            int y = 42;
            int size = 215;
            int borderRadius = 15;

            using (var bg = new MagickImage(Path.Combine(AppContext.BaseDirectory, "unknown.png")))
            {
                using (var image = profile.ProfilePic)
                {
                    image.Resize(new MagickGeometry() { Height = size, Width = size, IgnoreAspectRatio = true, FillArea = true });

                    using (var mask = new MagickImage(MagickColors.White, image.Width, image.Height))
                    {
                        new Drawables()
                        .FillColor(MagickColors.Black)
                        .StrokeColor(MagickColors.Black)
                        .Polygon(new PointD(0, 0), new PointD(0, borderRadius), new PointD(borderRadius, 0))
                        .Polygon(new PointD(mask.Width, 0), new PointD(mask.Width, borderRadius), new PointD(mask.Width - borderRadius, 0))
                        .Polygon(new PointD(0, mask.Height), new PointD(0, mask.Height - borderRadius), new PointD(borderRadius, mask.Height))
                        .Polygon(new PointD(mask.Width, mask.Height), new PointD(mask.Width, mask.Height - borderRadius), new PointD(mask.Width - borderRadius, mask.Height))
                        .FillColor(MagickColors.White)
                        .StrokeColor(MagickColors.White)
                        .Circle(borderRadius, borderRadius, borderRadius, 0)
                        .Circle(mask.Width - borderRadius, borderRadius, mask.Width - borderRadius, 0)
                        .Circle(borderRadius, mask.Height - borderRadius, 0, mask.Height - borderRadius)
                        .Circle(mask.Width - borderRadius, mask.Height - borderRadius, mask.Width - borderRadius, mask.Height)
                        .Draw(mask);

                        using (var imageAlpha = image.Clone())
                        {
                            imageAlpha.Alpha(AlphaOption.Extract);
                            imageAlpha.Opaque(MagickColors.White, MagickColors.None);
                            mask.Composite(imageAlpha, CompositeOperator.Over);
                        }

                        mask.HasAlpha = false;
                        image.HasAlpha = false;
                        image.Composite(mask, CompositeOperator.CopyAlpha);
                        bg.Composite(image, x, y, CompositeOperator.Over);
                    }
                }

                //draw xp bar
                float currentXp = profile.Xp;
                float toNextLvl = profile.XpToNextLevel;
                float xpPercent = currentXp / (currentXp + toNextLvl) * 100;

                int barX = 326;
                int barY = 210;
                int barWidth = 600;
                int barHeight = 20;
                float currentProgress = xpPercent / 100 * barWidth;

                var rects = new Drawables()
                    .FillColor(MagickColors.Transparent)
                    .StrokeColor(MagickColors.DimGray)
                    .Rectangle(barX, barY, barX + barWidth, barY + barHeight)
                    .FillColor(MagickColors.DimGray)
                    .Rectangle(barX, barY, barX + currentProgress, barY + barHeight);

                bg.Draw(rects);

                //Write info
                using (var caption = new MagickImage())
                {
                    var txtSettings = new MagickReadSettings
                    {
                        Font = "DejaVuSans.ttf",
                        FontPointsize = 32f,
                        BackgroundColor = MagickColors.Transparent,
                        FillColor = MagickColors.WhiteSmoke,
                        TextInterlineSpacing = 10,
                        Width = bg.Width
                    };
                    string text = $"Nivel {profile.Level}\nInsígnias {profile.Badges}\nGrupos {profile.GroupCount}";

                    caption.Read($"caption:{text}", txtSettings);
                    bg.Composite(caption, 708, 42, CompositeOperator.Over);

                    txtSettings.FillColor = MagickColors.SlateGray;

                    text = profile.PersonaName.Length < 15 ? profile.PersonaName : profile.PersonaName.Substring(0, 15);
                    caption.Read($"caption:{text}", txtSettings);
                    bg.Composite(caption, 326, 42, CompositeOperator.Over);

                    txtSettings.FillColor = MagickColors.WhiteSmoke;
                    txtSettings.FontPointsize = 26;
                    text = SteamStatus(profile.PersonaState);
                    caption.Read($"caption:{text}", txtSettings);
                    bg.Composite(caption, 326, 81, CompositeOperator.Over);

                    text = $"Jogos {profile.GameCount}";
                    caption.Read($"caption:{text}", txtSettings);
                    bg.Composite(caption, 326, 173, CompositeOperator.Over);

                    txtSettings.FontPointsize = 15;
                    text = $"{profile.Xp} XP → {profile.XpToNextLevel}";
                    caption.Read($"caption:{text}", txtSettings);
                    bg.Composite(caption, 326, 231, CompositeOperator.Over);
                }

                bg.Write(Path.Combine(AppContext.BaseDirectory, "overlay.png"));
            }
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
