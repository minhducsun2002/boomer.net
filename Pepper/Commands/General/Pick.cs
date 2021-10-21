using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Pepper.Commmands.General
{
    public class Pick : GeneralCommand
    {
        private readonly HttpClient httpClient;

        public Pick(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        [Command("pick", "choose")]
        [Description("Helps you throw a dice.")]
        public async Task<DiscordCommandResult> Exec([Remainder] string query = "")
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

            if (!int.TryParse(pick, out var result))
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