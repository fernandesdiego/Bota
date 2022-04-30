using Discord;
using Discord.Interactions;
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

        public async Task JoinAsync(SocketInteractionContext context)
        {
            if (!_lavaNode.IsConnected)
            {
                await context.Interaction.RespondAsync("```O player de audio está desconectado! Vou tentar reconectar. Tente rodar o comando novamente em alguns segundos```");
                await _lavaNode.ConnectAsync();
                return;
            }

            if (_lavaNode.HasPlayer(context.Guild) && (context.User as IVoiceState).VoiceChannel.Id != _lavaNode.GetPlayer(context.Guild).VoiceChannel.Id)
            {
                await context.Interaction.RespondAsync("Calma aí patrão, ja to conectado em outro canal de voz");
                return;
            }

            _lavaNode.TryGetPlayer(context.Guild, out var player);

            if (player != null)
            {
                if (player.VoiceChannel.Id == (context.User as IVoiceState).VoiceChannel.Id)
                {
                    await context.Interaction.RespondAsync("Já estou no mesmo canal de voz que você");
                    return;
                }
            }

            try
            {
                await _lavaNode.JoinAsync((context.User as IVoiceState).VoiceChannel, context.Interaction.Channel as ITextChannel);
                return;
            }
            catch (Exception e)
            {
                await context.Interaction.RespondAsync($"Algo de errado não está certo! Verifique os logs para mais informações. ```{DateTime.Now}\t{e.Message}```");
                return;
            }
        }

        public async Task PlayAsync(SocketInteractionContext context, string query)
        {   
            if (!_lavaNode.HasPlayer(context.Guild))
            {
                try
                {
                    await _lavaNode.JoinAsync((context.User as IVoiceState).VoiceChannel, context.Interaction.Channel as ITextChannel);
                }
                catch (Exception e)
                {
                    await context.Interaction.RespondAsync($"Algo de errado não está certo! Verifique os logs para mais informações. ```{DateTime.Now}\t{e.Message}```");
                    return;
                }
            }

            var player = _lavaNode.GetPlayer(context.Guild);

            if ((context.User as IVoiceState).VoiceChannel == player.VoiceChannel)
            {
                SearchResponse searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(query, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, query);

                if (searchResponse.Status == SearchStatus.LoadFailed || searchResponse.Status == SearchStatus.NoMatches)
                {
                    await context.Interaction.RespondAsync("Não encontrei nada disso aí que vc falou");
                    return;
                }

                // add to queue if already playing
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                    {
                        foreach (var track in searchResponse.Tracks)
                        {
                            player.Queue.Enqueue(track);
                        }

                        await context.Interaction.RespondAsync($"Adicionando {searchResponse.Tracks.Count} musicas na playlist.");
                        return;
                    }
                    else
                    {
                        player.Queue.Enqueue(searchResponse.Tracks.FirstOrDefault());
                        await context.Interaction.RespondAsync($"Adicionando {searchResponse.Tracks.FirstOrDefault().Title} na playlist.");
                        return;
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
                                await context.Interaction.RespondAsync($"Tocando agora: {searchResponse.Tracks.FirstOrDefault().Title}.");
                            }
                            else
                            {
                                player.Queue.Enqueue(searchResponse.Tracks.Where(x => x != firstTrack));
                            }
                        }

                        await context.Interaction.RespondAsync($"Adicionando {searchResponse.Tracks.Count} musicas na playlist.");
                        return;
                    }

                    // if it's a single track
                    else
                    {
                        await player.PlayAsync(firstTrack);
                        await context.Interaction.RespondAsync($"Tocando agora: {firstTrack.Title}");
                        return;
                    }
                }
            }
            await context.Interaction.RespondAsync("Você precisa estar no mesmo canal que eu");
        }

        public async Task PauseAsync(SocketInteractionContext context)
        {
            _lavaNode.TryGetPlayer(context.Guild, out var player);
            if (player == null)
            {
                await context.Interaction.RespondAsync("Não tem nada tocando agora... 🤔");
                return;
            }

            if ((context.User as IVoiceState).VoiceChannel == player.VoiceChannel)
            {
                if (player.PlayerState == PlayerState.Paused)
                {
                    await context.Interaction.RespondAsync("Já pausei o som, você quis dizer /resume?");
                    return;
                }

                if (player.PlayerState == PlayerState.Playing)
                {
                    await player.PauseAsync();
                    await context.Interaction.RespondAsync("Pausando música atual");
                    return;
                }
            }

            await context.Interaction.RespondAsync("Você precisa estar no mesmo canal que eu, para de ser troll kk");
        }

        public async Task ResumeAsync(SocketInteractionContext context)
        {
            _lavaNode.TryGetPlayer(context.Guild, out var player);
            if (player == null)
            {
                await context.Interaction.RespondAsync("Não tem nada tocando agora... 🤔");
                return;
            }

            if ((context.User as IVoiceState).VoiceChannel == player.VoiceChannel)
            {
                if (player.PlayerState == PlayerState.Playing)
                {
                    await context.Interaction.RespondAsync("O som ja está tocando.");
                    return;
                }

                if (player.PlayerState == PlayerState.Paused)
                {
                    await player.ResumeAsync();
                    await context.Interaction.RespondAsync("Continuando o som... 💽");
                    return;
                }
            }

            await context.Interaction.RespondAsync("Você precisa estar no mesmo canal que eu");
        }

        public async Task LeaveAsync(SocketInteractionContext context)
        {
            _lavaNode.TryGetPlayer(context.Guild, out var player);

            if (player == null)
            {
                await context.Interaction.RespondAsync("Nem to conectado oporra");
                return;
            }

            if ((context.User as IVoiceState).VoiceChannel == player.VoiceChannel)
            {
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    await player.StopAsync();
                }

                await _lavaNode.LeaveAsync(player.VoiceChannel);
                await context.Interaction.RespondAsync("Falouuuu");
                return;
            }

            await context.Interaction.RespondAsync("Você precisa estar no mesmo canal que eu");
        }

        public async Task ListAsync(SocketInteractionContext context)
        {
            _lavaNode.TryGetPlayer(context.Guild, out var player);

            if (player == null || player.PlayerState == PlayerState.Stopped)
            {
                await context.Interaction.RespondAsync("Não tem nada tocando agora chefia.");
                return;
            }

            if (player.Queue.Count < 1 && player.Track != null)
            {
                await context.Interaction.RespondAsync($"Tocando agora: {player.Track.Title}.\nEssa é a última musica da playlist.");
                return;
            }

            var embed = new EmbedBuilder().WithTitle("Playlist braba");
            embed.Description = $"-> {player.Track.Title}\n";
            for (int i = 0; i < player.Queue.Count; i++)
            {
                embed.Description += $"#{i + 1} - {player.Queue.ToList()[i].Title}\n";
            }

            await context.Interaction.RespondAsync(embed: embed.Build());
        }

        public async Task SkipAsync(SocketInteractionContext context)
        {
            _lavaNode.TryGetPlayer(context.Guild, out var player);

            if (player == null)
            {
                await context.Interaction.RespondAsync("Não tem nada tocando agora... 🤔");
                return;
            }

            if ((context.User as IVoiceState).VoiceChannel == player.VoiceChannel)
            {
                if (player.Queue.Count < 1)
                {
                    await context.Interaction.RespondAsync("Essa é a última música");
                    return;
                }

                var currentTrack = player.Track;

                await context.Interaction.RespondAsync($"Pulando {currentTrack.Title}");
                await player.SkipAsync();
                return;
            }
            await context.Interaction.RespondAsync("Você precisa estar conectado no mesmo canal que eu");
        }

        public async Task StopAsync(SocketInteractionContext context)
        {
            _lavaNode.TryGetPlayer(context.Guild, out var player);
            if (player == null)
            {
                await context.Interaction.RespondAsync("Não tem nada tocando agora... 🤔");
                return;
            }

            if ((context.User as IVoiceState).VoiceChannel == player.VoiceChannel)
            {
                if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
                {
                    await player.StopAsync();
                }

                await context.Interaction.RespondAsync("Parei e limpei a playlist.");
                return;
            }

            await context.Interaction.RespondAsync("Você precisa estar conectado no mesmo canal que eu");
        }

        private async Task TrackEnded(TrackEndedEventArgs args)
        {
            if (!(args.Reason == TrackEndReason.Finished))
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"Tocando agora: {track.Title}.");
        }
    }
}