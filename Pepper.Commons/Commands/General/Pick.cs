using Disqord;
using Disqord.Bot.Commands;
using Qmmands;
using Qmmands.Text;
using Random = System.Random;

namespace Pepper.Commons.Commands.General
{
    public class Pick : GeneralCommand
    {
        [TextCommand("pick", "choose")]
        [Description("Helps you throw a dice.")]
        public IDiscordCommandResult Exec(
            [Remainder][Description("What to choose from? Separate by `/`s.")] string query = ""
        )
        {
            var choices = query.Split("/").Where(piece => !string.IsNullOrWhiteSpace(piece)).ToArray();
            return choices.Length switch
            {
                0 => Reply("You gave me no choice!"),
                1 => Reply("You provided only one choice. `Choices / are / written / like / this`."),
                _ => Reply(new LocalEmbed
                {
                    Title = "And my choice is...", Description = choices[Random.Shared.Next(choices.Length)]
                })
            };
        }
    }
}