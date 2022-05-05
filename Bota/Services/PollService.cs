using Bota.Models;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bota.Services.Extensions;

namespace Bota.Services
{
    public class PollService
    {
        private readonly IServiceProvider _services;
        private readonly InteractionService _commands;
        private readonly DiscordSocketClient _client;
        private static List<Poll> Polls = new List<Poll>();

        public PollService(IServiceProvider services, InteractionService commands, DiscordSocketClient client)
        {
            _services = services;
            _commands = commands;
            _client = client;
            _client.ButtonExecuted += OnButtonExecuted;
        }

        private async Task OnButtonExecuted(SocketMessageComponent arg)
        {
            Poll clickedPoll = Polls.Find(p => p.MessageId == arg.Message.Id);
            if (clickedPoll == null)
            {
                await arg.RespondAsync("Erro ao computar voto.", ephemeral: true);
                return;
            }

            if (clickedPoll.Voters.Any(x => x.Id == arg.User.Id))
            {
                await arg.RespondAsync("Você já votou.", ephemeral: true);
                return;
            }

            var clickedButton = clickedPoll.Options.Keys.First(x => x.CustomId == arg.Data.CustomId);

            clickedPoll.Options[clickedButton]++;
            clickedPoll.Voters.Add(arg.User);

            var (embed, messageComponent) = BuildPollMessage(clickedPoll);

            await arg.UpdateAsync(m =>
            {
                m.Embed = embed;
                m.Components = messageComponent;
            });
        }

        public async Task CreatePoll(SocketInteractionContext commandContext, string question, int duration, string options, string description = null)
        {
            var optionStrings = options.Split(",", StringSplitOptions.TrimEntries);

            var btnDictionary = new Dictionary<ButtonComponent, int>();

            foreach (var option in optionStrings)
            {
                var b = new ButtonBuilder
                {
                    Label = option,
                    CustomId = Guid.NewGuid().ToString(),
                    Style = ButtonStyle.Primary,
                };
                btnDictionary.Add(b.Build(), 0);
            }

            var poll = new Poll(question, btnDictionary, DateTime.Now, duration, commandContext.Channel, description);

            poll.PollEnded += OnPollEnded;

            var pollMessage = BuildPollMessage(poll);

            await commandContext.Interaction.RespondAsync(embed: pollMessage.embed, components: pollMessage.messageComponent);
            var response = await commandContext.Interaction.GetOriginalResponseAsync();

            poll.MessageId = response.Id;

            Polls.Add(poll);
        }

        private static (Embed embed, MessageComponent messageComponent) BuildPollMessage(Poll poll)
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle($"📊 {poll.Question}")
                .WithDescription(poll.Description)
                .WithFooter($"Enquete termina em: {poll.StartTime.AddMinutes(poll.Duration)}");

            var totalVotes = poll.Options.Values.Sum();

            foreach (var option in poll.Options)
            {
                var optionPercent = totalVotes == 0 ? "0.00 %" : ((float)option.Value / (float)totalVotes).ToString("P");

                embedBuilder.AddField($"🔹 {option.Key.Label} - {option.Value} ({optionPercent})", "\u200b");
            }

            var components = new ComponentBuilder().FromButtons(poll.Options.Keys);

            return (embedBuilder.Build(), components.Build());
        }

        private void UpdatePoll(SocketInteractionContext context, SocketMessageComponent component)
        {

        }

        private async void OnPollEnded(object source, PollEventArgs args)
        {
            var message = await args.Poll.TextChannel.GetMessageAsync(args.Poll.MessageId);

            var embed = message.Embeds.First() as Embed;

            var mp = new MessageProperties()
            {
                Embed = embed,
                Components = new ComponentBuilder().Build(),
                Content = "Enquete encerrada",
            };

            await args.Poll.TextChannel.ModifyMessageAsync(message.Id, x => x.Components = new ComponentBuilder().Build());

            Polls.Remove(args.Poll);
        }
    }
}