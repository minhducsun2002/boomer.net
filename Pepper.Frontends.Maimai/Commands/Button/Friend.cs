using Disqord;
using Disqord.Bot.Commands.Components;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures.Exceptions;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public class Friend : MaimaiButtonCommand
    {
        private const string Name = "maifriend_1";
        private readonly MapiService mapiService;
        public Friend(MaimaiDxNetClientFactory f, MaimaiDataService d, IMaimaiDxNetCookieProvider c, MapiService mapiService) : base(f, d, c)
        {
            this.mapiService = mapiService;
        }

        [ButtonCommand($"{Name}:*")]
        public async Task Exec(ulong friendId)
        {
            var c = Context.AuthorId;
            await Context.Interaction.Response().DeferAsync();

            var cookie = await CookieProvider.GetCookie(c);
            var client = ClientFactory.Create(cookie);

            var friend = friendId.ToString();
            try
            {
                if (await client.AddFriend(friend))
                {
                    var i = await mapiService.Get(friend);

                    await Context.Interaction.Followup().SendAsync(
                        new LocalInteractionMessageResponse()
                            .WithContent($"{Context.Author.Mention}, a friend request was successfully sent to {i.Friend?.Name}.")
                    );
                }
                else
                {
                    var reasons = new[]
                    {
                        "- You/the person's friend/request limit is reached",
                        "- You/the person already sent a request"
                    };
                    await Context.Interaction.Followup().SendAsync(
                        new LocalInteractionMessageResponse()
                            .WithContent($"Failed to add {friend} as friend. Typical reasons :\n" + string.Join("\n", reasons))
                    );
                }
            }
            catch (Exception e)
            {
                if (e is LoginFailedException)
                {
                    await Context.Interaction.Followup().SendAsync(
                        new LocalInteractionMessageResponse()
                            .WithContent($"Sorry {Context.Author.Mention}, couldn't login :(")
                    );
                }
                else
                {
                    await Context.Interaction.Followup().SendAsync(
                        new LocalInteractionMessageResponse()
                            .WithContent($"Sorry {Context.Author.Mention}, something went wrong.")
                    );
                }
            }
        }

        public static string CreateCommand(ulong friendId) => $"{Name}:{friendId}";
    }
}