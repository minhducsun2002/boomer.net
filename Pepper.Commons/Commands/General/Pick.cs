using Disqord;
using Disqord.Bot.Commands;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commons.Commands.General
{
    public class Pick : GeneralCommand
    {
        private readonly HttpClient httpClient;

        public Pick(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        [TextCommand("pick", "choose")]
        [Description("Helps you throw a dice.")]
        public async Task<IDiscordCommandResult> Exec(
            [Remainder][Description("What to choose from? Separate by `/`s.")] string query = ""
        )
        {
            var choices = query.Split("/").Where(_ => !string.IsNullOrWhiteSpace(_)).ToArray();
            if (choices.Length == 0)
            {
                return Reply("You gave me no choice!");
            }

            if (choices.Length == 1)
            {
                return Reply("You provided only one choice. You probably don't need me to guess?");
            }

            var pick = await httpClient.GetStringAsync(
                $"https://www.random.org/integers/?num=1&min=1&max={choices.Length}&col=1&base=10&format=plain&rnd=new"
            );

            if (int.TryParse(pick, out var result))
            {
                return Reply(new LocalEmbed
                {
                    Title = "And my choice is...",
                    Description = choices[result - 1]
                });
            }

            return Reply(new LocalEmbed
            {
                Title = "And my choice is...",
                Description = choices[0]
            });
        }
    }
}