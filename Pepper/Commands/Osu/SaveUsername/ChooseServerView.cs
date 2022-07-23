using System;
using System.Threading.Tasks;
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
        private bool success;
        public ChooseServerView(string discordId, string username, IServiceProvider serviceProvider) : base(GenerateTemplateAction(username))
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
                    MessageTemplate = msg =>
                        msg.Content = $"<@{e.AuthorId}> is now bound to **{username}** on the {server.GetDisplayText()} server.";
                    success = true;
                    Menu.Stop();
                    ClearComponents();
                    await Menu.ApplyChangesAsync();
                })
                {
                    Label = server.GetDisplayText()
                });
            }
        }

        public override async ValueTask DisposeAsync()
        {
            if (!success)
            {
                MessageTemplate = msg => msg.Content = "You didn't choose the server in time. Try again.";
            }
            ClearComponents();
            await Menu.ApplyChangesAsync();
            await base.DisposeAsync();
        }

        private static Action<LocalMessageBase> GenerateTemplateAction(string username)
        {
            return msgBase =>
            {
                msgBase.Content = $"On which server is **{username}** your username?\nYou have 30 seconds to choose one.";
            };
        }
    }
}