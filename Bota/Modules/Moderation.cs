namespace Bota.Modules
{
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// The moderation module.
    /// </summary>
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Delete messages in the current channel.
        /// </summary>
        /// <param name="amount">The <see cref="amount"/> of messages to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} mensagens apagadas.");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }
    }
}
