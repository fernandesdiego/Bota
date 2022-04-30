using Bota.Context;
using Bota.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Responses.Search;

namespace Bota.Modules
{
    public class MusicModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly ApplicationContext _context;
        private readonly LavaNode _lavaNode;
        private readonly AudioService _audioService;
        private readonly SearchType[] searchTypes = { SearchType.YouTube, SearchType.YouTubeMusic, SearchType.SoundCloud };

        public InteractionService InteractionCommands { get; set; }

        public MusicModule(ApplicationContext context, LavaNode lavaNode, AudioService audioService, InteractionService interaction)
        {
            _context = context;
            _lavaNode = lavaNode;
            _audioService = audioService;
            InteractionCommands = interaction;
        }

        //[SlashCommand("join", "Chama o Bota pro canal de voz ao qual você está conectado")]
        //public async Task JoinAsync()
        //{
        //    if ((Context.User as IVoiceState).VoiceChannel != null)
        //    {
        //        await _audioService.JoinAsync(Context);
        //    }

        //    await Context.Interaction.RespondAsync("Você precisa estar conectado em um canal de voz.");
        //}

        [SlashCommand("leave", "Desliga o Bota do canal atual")]
        public async Task LeaveAsync()
        {
            if ((Context.User as IVoiceState).VoiceChannel != null)
            {
                await _audioService.LeaveAsync(Context);
            }

            await Context.Interaction.RespondAsync("Você precisa estar conectado em um canal de voz.");
        }

        [SlashCommand("play", "Toca uma musica de uma url ou pesquisa ela pelo texto informado")]
        public async Task PlayAsync([Remainder] string song)
        {
            if ((Context.User as IVoiceState).VoiceChannel != null)
            {
                await _audioService.PlayAsync(Context, song);
            }

            await Context.Interaction.RespondAsync("Você precisa estar conectado em um canal de voz.");
        }

        [SlashCommand("pause", "Pausa a música atual")]
        public async Task PauseAsync()
        {
            if ((Context.User as IVoiceState).VoiceChannel != null)
            {
                await _audioService.PauseAsync(Context);
            }

            await Context.Interaction.RespondAsync("Você precisa estar conectado em um canal de voz.");
        }

        [SlashCommand("resume", "Continua a tocar a última musica que foi pausada")]
        public async Task ResumeAsync()
        {
            if ((Context.User as IVoiceState).VoiceChannel != null)
            {
                await _audioService.ResumeAsync(Context);
            }

            await Context.Interaction.RespondAsync("Você precisa estar conectado em um canal de voz.");
        }

        [SlashCommand("stop", "Para de reproduzir o áudio atual")]
        public async Task StopAsync()
        {
            if ((Context.User as IVoiceState).VoiceChannel != null)
            {
                await _audioService.StopAsync(Context);
            }

            await Context.Interaction.RespondAsync("Você precisa estar conectado em um canal de voz.");
        }

        [SlashCommand("skip", "Pula a música atual")]
        public async Task SkipAsync()
        {
            if ((Context.User as IVoiceState).VoiceChannel != null)
            {
                await _audioService.SkipAsync(Context);
            }

            await Context.Interaction.RespondAsync("Você precisa estar conectado em um canal de voz.");
        }

        [SlashCommand("list", "Mostra a fila de músicas")]
        public async Task ListAsync()
        {
            await _audioService.ListAsync(Context);
        }
    }
}
