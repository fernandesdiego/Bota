using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bota.Modules
{
    public class FunModule : InteractionModuleBase<SocketInteractionContext>
    {
        public InteractionService InteractionCommands { get; set; }
        public FunModule(InteractionService interaction)
        {
            InteractionCommands = interaction;
        }

        [SlashCommand("roll", "Joga um dado com o número de lados especificado")]
        public async Task RollDice(int qtd, int sides)
        {
            int[] dices = new int[qtd];
            var rand = new Random();

            var eb = new EmbedBuilder()
                .WithTitle($"Jogando {qtd} D{sides}")
                .WithDescription("Resultado...");

            for (int i = 0; i < qtd; i++)
            {
                eb.AddField("🎲", rand.Next(1, sides), true);
            }

            await Context.Interaction.RespondAsync(embed: eb.Build());
        }
    }
}
