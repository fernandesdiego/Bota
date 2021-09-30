namespace Bota.Modules
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Bota.Context;
    using Bota.Models;
    using Bota.Services;
    using Discord;
    using Discord.Commands;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Memes and fun commands.
    /// </summary>
    public class Fun : ModuleBase<SocketCommandContext>
    {

        private readonly ApplicationContext _context;
        private readonly SteamService _steamService;
        public Fun(ApplicationContext context, SteamService steamService)
        {
            _context = context;
            _steamService = steamService;
        }

        /// <summary>
        /// Gets a random meme from Reddit.
        /// </summary>
        /// <param name="subreddit">The <see cref="subreddit"/> to search from.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("meme")]
        [Alias("reddit")]
        public async Task Meme(string subreddit = "memes")
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("Não encontrei esse subreddit.");
            }

            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            var embed = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(255, 255, 0))
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"💬 {post["num_comments"]} ⬆️ {post["ups"]}")
                .Build();
            await Context.Channel.SendMessageAsync(embed: embed);
        }

        [Command("steam")]
        public async Task GetUserById(long steamId)
        {
            await _steamService.GetSteamUser(Context,steamId: steamId);

            // await GetSteamUser(steamId: steamId);
        }

        [Command("steam")]
        public async Task GetUserByName(string userName)
        {
            await _steamService.GetSteamUser(Context, userName: userName);

            // await GetSteamUser(userName: userName);
        }

        //public async Task GetSteamUser(long steamId = 0, string userName = "")
        //{
        //    var config = await _context.BotConfigs.FirstOrDefaultAsync();
        //    var steamApiKey = config.SteamApiKey;
        //    if (steamApiKey == string.Empty || steamApiKey == null)
        //    {
        //        await Context.Channel.SendMessageAsync("Configure a sua API key da Steam no painel primeiro");
        //    }

        //    if (steamId == 0 && userName == string.Empty)
        //    {
        //        await Context.Channel.SendMessageAsync("Preciso do nome de usuário ou Id.");
        //    }
        //    if (steamId == 0 && userName != string.Empty)
        //    {
        //        steamId = await GetSteamByName(userName);
        //        if (steamId == 0)
        //        {
        //            await Context.Channel.SendMessageAsync("Não encontrei esse usuário.");
        //            return;
        //        }
        //    }

        //    var client = new HttpClient();
        //    var result = await client.GetStringAsync($"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={steamApiKey}&steamids={steamId}");

        //    JObject obj = JObject.Parse(result);
        //    JArray arr = JArray.Parse(obj["response"]["players"].ToString());

        //    if (arr.Count == 0)
        //    {
        //        await Context.Channel.SendMessageAsync("Não encontrei esse usuário.");
        //        return;
        //    }

        //    SteamProfileInfo profile = JsonConvert.DeserializeObject<SteamProfileInfo>(arr[0].ToString());

        //}

        //private async Task<long> GetSteamByName([Remainder]string steamName)
        //{
        //    var config = await _context.BotConfigs.FirstOrDefaultAsync();
        //    var steamApiKey = config.SteamApiKey;
        //    if (steamApiKey == string.Empty || steamApiKey == null)
        //    {
        //        await Context.Channel.SendMessageAsync("Configure a sua API key da Steam no painel primeiro");
        //    }

        //    var client = new HttpClient();
        //    var result = await client.GetStringAsync($"https://api.steampowered.com/ISteamUser/ResolveVanityURL/v0001/?key={steamApiKey}&vanityurl={steamName}");
        //    JObject obj = JObject.Parse(result);
        //    long.TryParse((string)obj["response"]["steamid"], out long id);
        //    return id;
        //}

        //private static string SteamStatus(int number)
        //{
        //    switch (number)
        //    {
        //        case 0:
        //            return "Offline";
        //        case 1:
        //            return "Online";
        //        case 2:
        //            return "Ocupado";
        //        case 3:
        //            return "Away";
        //        default:
        //            return "Desconhecido";
        //    }
        //}
    }
}
