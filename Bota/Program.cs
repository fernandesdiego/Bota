namespace Bota
{
    using System.IO;
    using System.Threading.Tasks;
    using Bota.Services;
    using Discord;
    using Discord.Addons.Hosting;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Bota.Context;
    using Microsoft.EntityFrameworkCore;
    using Victoria;
    using System;
    using System.Diagnostics;
    using Discord.Interactions;
    using Bota.Services.Configuration;

    /// <summary>
    /// Entry point of the program.
    /// </summary>
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(x => x
                        .SetBasePath(GetBasePath())
                        .Build())
                    .ConfigureLogging(x => x
                        .AddConsole()
                        .SetMinimumLevel(LogLevel.Trace))
                    .ConfigureServices((context, services) =>
                    {
                        services.AddLavaNode(options =>
                        {
                            options.SelfDeaf = false;
                            options.LogSeverity = LogSeverity.Debug;
                            options.Authorization = context.Configuration["LavaNodePwd"];
                            options.Port = 2333;
                            options.Hostname = context.Configuration["LavalinkServer"];
                        });

                        services.AddSingleton<AudioService>();
                        services.AddSingleton<PollService>();
                        services.AddSingleton<SteamService>();
                        services.Configure<SteamSettings>(context.Configuration.GetSection("SteamSettings"));

                        services.AddHostedService<InteractionHandler>();

                    })
                    .ConfigureDiscordHost((context, config) =>
                    {
                        config.SocketConfig = new DiscordSocketConfig
                        {
                            LogLevel = LogSeverity.Debug,
                            AlwaysDownloadUsers = true,
                            MessageCacheSize = 200,
                        };
                        config.Token = context.Configuration["Token"];
                    })
                    .UseInteractionService((context, config) =>
                    {
                        config.LogLevel = LogSeverity.Debug;
                        config.DefaultRunMode = Discord.Interactions.RunMode.Async;
                    })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }

        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }
    }
}
