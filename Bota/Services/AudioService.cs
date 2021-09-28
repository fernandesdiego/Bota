using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace Bota.Services
{
    public class AudioService
    {
        private readonly LavaNode _lavaNode;

        public AudioService(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
            _lavaNode.OnTrackEnded += TrackEnded;
        }

        public async Task JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild) && voiceState.VoiceChannel == _lavaNode.GetPlayer(guild).VoiceChannel)
            {
                await textChannel.SendMessageAsync("Calma aí patrão, ja to conectado no canal de voz");
                return;
            }

            if (voiceState.VoiceChannel == null)
            {
                await textChannel.SendMessageAsync("Você precisa estar conectado em um canal de voz");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);
                await textChannel.SendMessageAsync($"Entrei no canal {voiceState.VoiceChannel.Name}");
            }
            catch (Exception e)
            {
                await textChannel.SendMessageAsync($"Deu ruim => {e.Message}");
                return;
            }
        }

        public async Task PlayAsync(SocketGuildUser user, IGuild guild, IVoiceState voiceState, ITextChannel textChannel, string query)
        {
            if (!_lavaNode.HasPlayer(guild))
            {
                await JoinAsync(guild, voiceState, textChannel);
            }

            SearchResponse searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(query, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, query);

            if (searchResponse.Status == SearchStatus.LoadFailed || searchResponse.Status == SearchStatus.NoMatches)
            {
                await textChannel.SendMessageAsync("Não encontrei nada disso aí que vc falou");
                return;
            }

            var player = _lavaNode.GetPlayer(guild);

            // add to queue if already playing
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    foreach (var track in searchResponse.Tracks)
                    {
                        player.Queue.Enqueue(track);
                    }

                    await textChannel.SendMessageAsync($"Adicionando {searchResponse.Tracks.Count} musicas na playlist.");
                }
                else
                {
                    player.Queue.Enqueue(searchResponse.Tracks.FirstOrDefault());
                    await textChannel.SendMessageAsync($"Adicionando {searchResponse.Tracks.FirstOrDefault().Title} na playlist.");
                }
            }

            // if not playing already
            else
            {
                var firstTrack = searchResponse.Tracks.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    for (var i = 0; i < searchResponse.Tracks.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(firstTrack);
                            await textChannel.SendMessageAsync($"Tocando agora: {searchResponse.Tracks.FirstOrDefault().Title}.");
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks.Where(x => x != firstTrack));
                        }
                    }

                    await textChannel.SendMessageAsync($"Adicionando {searchResponse.Tracks.Count} musicas na playlist.");
                }

                // if it's a single track
                else
                {
                    await player.PlayAsync(firstTrack);
                    await textChannel.SendMessageAsync($"Tocando agora: {firstTrack.Title}");
                }
            }
        }

        public async Task LeaveAsync(IGuild guild, ITextChannel textChannel)
        {
            var player = _lavaNode.GetPlayer(guild);

            if (player == null)
            {
                textChannel.SendMessageAsync("Nem to conectado oporra");
                return;
            }

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                await player.StopAsync();
            }

            await _lavaNode.LeaveAsync(player.VoiceChannel);
            await textChannel.SendMessageAsync("Falouuuuu");
        }
        public async Task ListAsync(IGuild guild, ITextChannel textChannel)
        {
            var player = _lavaNode.GetPlayer(guild);
            if (player == null || player.PlayerState == PlayerState.Stopped)
            {
                await textChannel.SendMessageAsync("Não tem nada tocando agora chefia.");
                return;
            }

            if (player.Queue.Count < 1 && player.Track != null)
            {
                await textChannel.SendMessageAsync($"Tocando agora: {player.Track.Title}.\nEssa é a última musica da playlist.");
                return;
            }

            var embed = new EmbedBuilder().WithTitle("Playlist braba");
            embed.Description = ($"-> {player.Track.Title}\n");
            for (int i = 0; i < player.Queue.Count; i++)
            {
                embed.Description += $"#{i + 1} - {player.Queue.ToList()[i].Title}\n";
            }

            await textChannel.SendMessageAsync(embed: embed.Build());
        }

        public async Task SkipAsync(IGuild guild, ITextChannel textChannel)
        {
            var player = _lavaNode.GetPlayer(guild);

            if (player == null)
            {
                textChannel.SendMessageAsync("Não achei o player, tem alguma coisa tocando? 🤔");
                return;
            }

            if (player.Queue.Count < 1)
            {
                textChannel.SendMessageAsync("Essa é a última música");
                return;
            }

            var currentTrack = player.Track;

            textChannel.SendMessageAsync($"Pulando {currentTrack.Title}");
            await player.SkipAsync();
        }

        public async Task StopAsync(IGuild guild, ITextChannel textChannel)
        {
            var player = _lavaNode.GetPlayer(guild);
            if (player == null)
            {
                textChannel.SendMessageAsync("Não achei o player, tem alguma coisa tocando? 🤔");
            }

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                await player.StopAsync();
            }

            textChannel.SendMessageAsync("Parei e limpei a playlist.");
        }

        private async Task TrackEnded(TrackEndedEventArgs args)
        {
            if (!(args.Reason == TrackEndReason.Finished))
            {
                return;
            }

            var player = args.Player;
            if(!player.Queue.TryDequeue(out var queueable))
            {
                return;
            }

            if(!(queueable is LavaTrack track))
            {
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"Tocando agora: {track.Title}.");
        }
    }
}