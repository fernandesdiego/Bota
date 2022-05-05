using Discord;
using Discord.Addons.Hosting;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Victoria;

namespace Bota.Services
{
    public class InteractionHandler : DiscordClientService
    {
        private readonly IServiceProvider _services;
        private readonly InteractionService _commands;
        private readonly DiscordSocketClient _client;
        private readonly LavaNode _lavaNode;

        public InteractionHandler(IServiceProvider services, InteractionService commands, DiscordSocketClient client, LavaNode lavaNode, ILogger<DiscordClientService> logger) : base(client, logger)
        {
            _services = services;
            _commands = commands;
            _client = client;
            _lavaNode = lavaNode;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.InteractionCreated += HandleInteraction;
            _client.Ready += OnReady;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task OnReady()
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
#if DEBUG
            await _commands.RegisterCommandsToGuildAsync(889915217163735081, true);
#else
            await _commands.RegisterCommandsGloballyAsync(true);
#endif
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var context = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(context, _services);
            }
            catch (Exception ex)
            {
                if (arg.Type == InteractionType.ApplicationCommand)
                {
                    await arg.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }

                Console.WriteLine(ex);
            }
        }
    }
}
