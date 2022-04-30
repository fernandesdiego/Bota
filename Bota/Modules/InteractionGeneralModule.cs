using Bota.Services;
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


        public InteractionGeneralModule(InteractionService interaction)
        {
            InteractionCommands = interaction;
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
    }
}
