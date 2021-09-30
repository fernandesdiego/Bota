using Bota.Context;
using Bota.Models;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
                await commandContext.Channel.SendMessageAsync("Não encontrei esse usuário.");
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

            Console.Write(profile);
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
