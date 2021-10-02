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

    /// <summary>
    /// Entry point of the program.
    /// </summary>
    internal class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x => x
                    //.SetBasePath(GetBasePath())
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                    .Build())
                .ConfigureLogging(x => x
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug))
                .ConfigureServices((context, services) =>
                {
                    var connectionString = context.Configuration.GetConnectionString("DefaultConnectionString");
                    var serverversion = ServerVersion.AutoDetect(connectionString);
                    services.AddDbContext<ApplicationContext>(options => options.UseMySql(connectionString, serverversion));
                    services.AddSingleton<LavaNode>();
                    services.AddSingleton<LavaConfig>();
                    services.AddSingleton<AudioService>();
                    services.AddSingleton<SteamService>();
                    services.AddLavaNode(x =>
                    {
                        x.SelfDeaf = false;
                        x.LogSeverity = LogSeverity.Debug;
                    });
                    services.AddHostedService<CommandHandler>();
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
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Debug;
                    config.DefaultRunMode = RunMode.Sync;
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
