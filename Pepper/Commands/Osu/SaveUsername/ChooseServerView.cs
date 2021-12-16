using System;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Structures.External.Osu.Extensions;

namespace Pepper.Commands.Osu
{
    public class ChooseServerView : ViewBase
    {
        public ChooseServerView(string discordId, string username, IServiceProvider serviceProvider) : base(GenerateMessage(username))
        {
            foreach (var server in Enum.GetValues<GameServer>())
            {
                AddComponent(new ButtonViewComponent(async e =>
                {
                    await e.Interaction.Response().DeferAsync();
                    var record = new Username { DiscordUserId = discordId };
                    switch (server)
                    {
                        case GameServer.Osu:
                            record.OsuUsername = username;
                            break;
                        case GameServer.Ripple:
                            record.RippleUsername = username;
                            break;
                    }

                    var usernameProvider = serviceProvider.GetRequiredService<IOsuUsernameProvider>();
                    await usernameProvider.StoreUsername(record);
                    TemplateMessage.Content = $"<@{e.AuthorId}> is now bound to **{username}** on the {server.GetDisplayText()} server.";
                    Menu.Stop();
                    ClearComponents();
                    await Menu.ApplyChangesAsync();
                })
                {
                    Label = server.GetDisplayText()
                });
            }
        }

        private static LocalMessage GenerateMessage(string username)
        {
            return new LocalMessage().WithContent($"On which server is **{username}** your username?\nYou have 30 seconds to choose one.");
        }
    }
}