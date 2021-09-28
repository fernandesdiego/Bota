namespace Bota.Commons
{
    using Discord;

    /// <summary>
    /// Custom embed builder with themes.
    /// </summary>
    internal class BotaEmbedBuilder : EmbedBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BotaEmbedBuilder"/> class.
        /// </summary>
        public BotaEmbedBuilder()
        {
            WithColor(new Color(255, 255, 0));
        }
    }
}
