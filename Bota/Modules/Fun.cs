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
        public async Task GetUserById(int steamId)
        {
            await _steamService.GetSteamUser(commandContext: Context, id3: steamId);
        }
    }
}
