namespace Bota.Modules
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bota.Commons;
    using Bota.Services;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// The general module containing general commands (duh).
    /// </summary>
    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _command;
        public General(CommandService command)
        {
            _command = command;
        }
        /// <summary>
        /// Respond with pong.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("ping")]
        [Alias("teste")]
        [RequireBotPermission(ChannelPermission.SendMessages)]
        [Summary("Se eu estiver online responderei 'Pong'")]
        public async Task PingAsync()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }

        /// <summary>
        /// Get some basic information about a user.
        /// </summary>
        /// <param name="socketGuildUser">An optional user to get information from.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("info")]
        public async Task InfoAsync(SocketGuildUser socketGuildUser = null)
        {
            if (socketGuildUser == null)
            {
                socketGuildUser = Context.User as SocketGuildUser;
            }

            var embed = new EmbedBuilder()
                .WithTitle($"{socketGuildUser.Username}#{socketGuildUser.Discriminator}")
                .AddField("ID", socketGuildUser.Id, true)
                .AddField("Criado em", socketGuildUser.CreatedAt.ToLocalTime().ToString(format: "dddd, dd MMMM yyyy"), true)
                .AddField("Entrou em", socketGuildUser.JoinedAt.Value.ToLocalTime().ToString(format: "dddd, dd MMMM yyyy"), true)
                .AddField("Cargos", string.Join("|", socketGuildUser.Roles.Select(x => x.Name)))
                .WithColor(255, 255, 0)
                .WithThumbnailUrl(socketGuildUser.GetAvatarUrl() ?? socketGuildUser.GetDefaultAvatarUrl())
                .Build();

            await ReplyAsync(embed: embed);
        }

        /// <summary>
        /// Get basic information about the current server.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("server")]
        public async Task ServerInfoAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle($"Informações sobre {Context.Guild.Name}")
                .AddField("Criado em", value: Context.Guild.CreatedAt.ToLocalTime().ToString(format: "dddd, dd MMMM yyyy"), true)
                .AddField("Membros", Context.Guild.MemberCount, true)
                .AddField("Usuários Online", Context.Guild.Users.Where(x => x.Status != UserStatus.Offline).Count(), true)
                .WithColor(255, 255, 0)
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .Build();

            await ReplyAsync(embed: embed);
        }

    }
}
