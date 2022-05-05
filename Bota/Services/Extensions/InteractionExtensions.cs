using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Bota.Services.Extensions
{
    public static class InteractionExtensions
    {
        public static ComponentBuilder FromButtons(this ComponentBuilder component, IReadOnlyCollection<ButtonComponent> components)
        {
            ComponentBuilder builder = new ComponentBuilder();
            foreach (var item in components)
            {
                builder.WithButton(item.Label, item.CustomId, item.Style);
            }

            return builder;
        }
    }
}
