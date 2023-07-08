using Disqord;
using Disqord.Bot.Commands;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Pepper.Frontends.Maimai.Structures.External.Mapi;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class Friend : MaimaiTextCommand
    {
        private readonly MapiService mapiService;
        public Friend(MaimaiDxNetClientFactory f, MaimaiDataService d, IMaimaiDxNetCookieProvider c, MapiService s) : base(f, d, c)
        {
            mapiService = s;
        }

        [Description("Search for friends!")]
        [TextCommand("friend")]
        [OverloadPriority(0)]
        public async Task<IDiscordCommandResult> Exec([Description("Friend code to check.")] string friendCode = "")
        {
            friendCode = friendCode.Trim();

            if (string.IsNullOrWhiteSpace(friendCode))
            {
                return Reply("What? Gimme a friend code, or else!");
            }

            string? err = null;
            for (var i = 0; i < 5; i++)
            {
                FriendResponse user;
                try
                {
                    user = await mapiService.Get(friendCode);
                }
                catch
                {
                    return Reply("Something gone wrong. Please try again :sob:");
                }
                if (!user.Success)
                {
                    err = user.Error;
                    continue;
                }

                var friend = user.Friend!;
                var embed = new LocalEmbed()
                    .WithAuthor(friend.Name)
                    .WithThumbnailUrl(friend.Avatar)
                    .WithDescription($"**{friend.Rating}** - {(SeasonClass) friend.Class}")
                    .WithFooter($"ID : {friendCode}");

                var msg = new LocalMessage()
                    .WithEmbeds(embed)
                    .WithComponents(
                        LocalComponent.Row(
                            LocalComponent.Button(
                                Button.Friend.CreateCommand(ulong.Parse(friendCode)),
                                "Add friend"
                            )
                        )
                    );

                return Reply(msg);
            }

            return Reply($"Error trying friend code {friendCode} : {err ?? "(unknown error)"}");
        }

        [Description("Search for friends!")]
        [TextCommand("friend")]
        [OverloadPriority(1)]
        public async Task<IDiscordCommandResult> Exec([Description("Friend code to check.")] IMember? user = null)
        {
            var uid = user?.Id ?? Context.AuthorId;
            var u = await CookieProvider.GetFriendId(uid);
            if (u is null)
            {
                return Reply($"<@{uid}> has not logged in, can't get their friend code :(");
            }

            var cookie = await CookieProvider.GetCookie(uid);
            var client = ClientFactory.Create(cookie!);
            if (u is 0)
            {
                var r = await Reply("Trying to get user friend code...");
                var f = await client.GetUserFriendCode();
                var _ = CookieProvider.StoreCookie(uid, cookie!, f);
                u = f;
                _ = r.DeleteAsync();
            }

            return await Exec(u.ToString()!);
        }
    }
}