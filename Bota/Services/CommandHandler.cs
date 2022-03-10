namespace Bota.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Bota.Context;
    using Discord;
    using Discord.Addons.Hosting;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Victoria;

    /// <summary>
    /// Class to handling the commands and various events.
    /// </summary>
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly ApplicationContext _applicationContext;
        private readonly LavaNode _lavaNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> that should be injected.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> that should be injected.</param>
        /// <param name="client">The <see cref="DiscordSocketClient"/> that should be injected.</param>
        /// <param name="service">The <see cref="CommandService"/> that should be injected.</param>
        /// <param name="logger">The <see cref="ILogger"/> that should be injected.</param>
        public CommandHandler(IServiceProvider provider, IConfiguration configuration, DiscordSocketClient client, CommandService service, ApplicationContext applicationContext, LavaNode lavaNode,ILogger<DiscordClientService> logger)
            : base(client, logger)
        {
            _provider = provider;
            _configuration = configuration;
            _client = client;
            _service = service;
            _applicationContext = applicationContext;
            _lavaNode = lavaNode;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _client.JoinedGuild += OnJoinedGuild;
            _client.UserJoined += OnUserJoined;
            _client.UserLeft += OnUserLeft;
            _client.Ready += OnReady;
            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnReady()
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
        }

        private async Task OnUserLeft(SocketGuild socketGuild, SocketUser socketUser)
        {
            // TODO: get message list from db, return string.format("'string {0}', var)" with mention;
            List<string> leftMessages = new ()
            {
                $"Ue, {socketUser.Mention} ficou puto e kitou?",
                $"Que que houve com o {socketUser.Mention}?",
                $"Mas ja vai, {socketUser.Mention}?",
                $"Vai tarde, {socketUser.Mention}...",
                $"Beleza então, {socketUser.Mention}, pra entrar dnv vai ter que dar uma mamada",
                $"{socketUser.Mention}: 'Ain fiquei putinho e kitei'",
                $"{socketUser.Mention} é corno",
            };

            var rand = new Random();

            await socketGuild.DefaultChannel.SendMessageAsync(leftMessages[rand.Next(0, leftMessages.Count)]);
        }

        private async Task OnUserJoined(SocketGuildUser socketGuildUser)
        {
            // TODO: get message from db, return string.format("'string {0}', var)" with mention;
            await socketGuildUser.Guild.DefaultChannel.SendMessageAsync($"Ae {socketGuildUser.Mention}, bem vindo ao meu canal, corno. Pega na minha e balança aqui 🤝 \n uiui");
        }

        private async Task OnJoinedGuild(SocketGuild socketGuild)
        {
            // TODO: get message list from db
            await socketGuild.DefaultChannel.SendMessageAsync("Cheguei nessa porra, ");
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
        {
            if (result.IsSuccess)
            {
                return;
            }


            await commandContext.Channel.SendMessageAsync(result.ErrorReason);
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message)
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            var argPos = 0;
            var config = await _applicationContext.BotConfigs.FirstOrDefaultAsync();

            if (!message.HasStringPrefix(config.Prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(_client, message);

            await context.Channel.TriggerTypingAsync();

            await _service.ExecuteAsync(context, argPos, _provider);
        }
    }
}
