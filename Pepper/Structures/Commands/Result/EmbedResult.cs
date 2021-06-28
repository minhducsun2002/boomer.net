using System;
using Discord;
using Qmmands;

namespace Pepper.Structures.Commands.Result
{
    public class EmbedResult : CommandResult
    {
        public override bool IsSuccessful => true;
        public Embed[] Embeds = Array.Empty<Embed>();
        public Embed DefaultEmbed = new EmbedBuilder{ Description = "The command didn't return any result." }.Build();
    }
}