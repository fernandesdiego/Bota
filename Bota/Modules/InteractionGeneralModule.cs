using Bota.Services;
using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bota.Modules
{
    public class InteractionGeneralModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService InteractionCommands { get; set; }

        private readonly PollService _pollService;

        public InteractionGeneralModule(InteractionService interaction, PollService pollService)
        {
            InteractionCommands = interaction;
            _pollService = pollService;
        }

        [SlashCommand("ping", "Retorna pong")]
        public async Task Ping()
        {
            await Context.Interaction.RespondAsync("pong");
        }

        [SlashCommand("echo", "Repete o input")]
        public async Task Echo(string message)
        {
            await Context.Interaction.RespondAsync(message);
        }

        [SlashCommand("help", "Mostra comandos disponíveis")]
        public async Task Help()
        {
            var commands = InteractionCommands.SlashCommands.ToList();
            string info = "Lista de comandos:\n";
            foreach (var command in commands)
            {
                info += $"Nome: {command.Name}\tDescrição: {command.Description}\n";
            }

            await Context.Interaction.RespondAsync(info);
        }

        [SlashCommand("votacao", "Cria uma votação")]
        public async Task Poll([Summary("questão", "Qual a questão da sua votação?")] string question, [Summary("duracao", "Quanto tempo vai durar sua votação")] int duration, [Summary("opcoes", "As opções disponíveis separadas por virgula")] string options, [Summary("descrição", "Descrição opcional da sua votação")] string description = null)
        {
            await _pollService.CreatePoll(Context, question, duration, options, description);
        }

        [SlashCommand("teste", "testando")]
        public async Task Test()
        {
            var eb = new EmbedBuilder().WithTitle("EmbedTest");
            var cb = new ComponentBuilder().WithButton("ComponentButton", Guid.NewGuid().ToString());
            cb.WithButton("SecondComponentButton", Guid.NewGuid().ToString());
            await Context.Interaction.RespondAsync(embed: eb.Build(), components: cb.Build());
        }
    }
}