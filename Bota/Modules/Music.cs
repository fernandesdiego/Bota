using Bota.Context;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Discord.WebSocket;
using System.Linq;
using Victoria.Responses.Search;
using Bota.Services;

namespace Bota.Modules
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly ApplicationContext _context;
        private readonly LavaNode _lavaNode;
        private readonly AudioService _audioService;
        private readonly SearchType[] searchTypes = { SearchType.YouTube, SearchType.YouTubeMusic, SearchType.SoundCloud };

        public Music(ApplicationContext context, LavaNode lavaNode, AudioService audioService)
        {
            _context = context;
            _lavaNode = lavaNode;
            _audioService = audioService;
        }

        [Command("join")]
        [Alias("entrar")]
        public async Task JoinAsync()
        {
            await _audioService.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel);
        }
        [Command("leave")]
        public async Task LeaveAsync()
        {
            await _audioService.LeaveAsync(Context.Guild, Context.Channel as ITextChannel);
        }

        [Command("play")]
        public async Task PlayAsync([Remainder] string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                await ReplyAsync("Passa o link 🔫, ou o nome da musica pra eu procurar");
                return;
            }

            await _audioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel, searchQuery);
        }
        
        [Command("stop")]
        public async Task StopAsync()
        {
            await _audioService.StopAsync(Context.Guild, Context.Channel as ITextChannel);
        }
        [Command("skip")]
        public async Task SkipAsync()
        {
            await _audioService.SkipAsync(Context.Guild, Context.Channel as ITextChannel);
        }

        [Command("list")]
        public async Task ListAsync()
        {
            await _audioService.ListAsync(Context.Guild, Context.Channel as ITextChannel);
        }
    }
}
