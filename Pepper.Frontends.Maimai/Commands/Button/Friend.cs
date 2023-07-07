using Disqord;
using Disqord.Bot.Commands.Components;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public class Friend : MaimaiButtonCommand
    {
        private const string Name = "maifriend_1";
        public Friend(MaimaiDxNetClientFactory f, MaimaiDataService d, IMaimaiDxNetCookieProvider c) : base(f, d, c) {}

        [ButtonCommand($"{Name}:*")]
        public async Task Exec(ulong friendId)
        {
            var c = Context.AuthorId;
            await Context.Interaction.Response().DeferAsync();
            
            var cookie = await CookieProvider.GetCookie(c);
            var client = ClientFactory.Create(cookie);

            if (await client.AddFriend(friendId.ToString()))
            {
                await Context.Interaction.Followup().SendAsync(
                    new LocalInteractionMessageResponse()
                        .WithContent("done")
                );  
            }
        }

        public static string CreateCommand(ulong friendId) => $"{Name}:{friendId}";
    }
}