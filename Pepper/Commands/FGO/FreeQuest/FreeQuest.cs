using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO.Entities;
using Pepper.Structures.External.FGO.TypeParsers;
using Qmmands;

namespace Pepper.Commands.FGO
{
    internal class QuestIdentityCheckAttribute : ParameterCheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
        {
            var questId = (QuestIdentity) argument;
            var quest = context.Services.GetRequiredService<MasterDataService>()
                .Connections[Region.NA]
                .ResolveQuest(questId);
            return quest != null ? Success() : Failure("Not a valid quest ID");
        }
    }

    internal class QuestIdentityCheckFailureFormatterAttribute : Attribute, IParameterCheckFailureFormatter
    {
        public LocalMessage? FormatFailure(
            ParameterChecksFailedResult parameterChecksFailedResult,
            DiscordCommandContext commandContext
        )
            => new LocalMessage().WithContent(
                $"ID {parameterChecksFailedResult.Argument} does not resolve to a valid quest."
            );
    }

    public partial class FreeQuest : FGODataCommand
    {
        public FreeQuest(MasterDataService m, TraitService t) : base(m, t) { }

        [Command("show-free-quest", "q", "ssq", "sfq", "sq")]
        [Description("Show information about a free quest.")]
        public async Task<DiscordCommandResult> Exec(
            [QuestIdentityCheck][QuestIdentityCheckFailureFormatter][Description("A quest ID or quest name.")] QuestIdentity questResolvable
        )
        {
            var na = MasterDataService.Connections[Region.NA];
            var quest = na.ResolveQuest(questResolvable)!;
            var spot = na.ResolveSpot(quest.SpotId)!;
            var war = na.ResolveWar(spot.WarId)!;
            var phasesTasks = na.ListQuestPhases(questResolvable)
                .OrderBy(quest => quest.Phase)
                .Select(phase => Query(Region.NA, questResolvable, phase.Phase));

            var data = (await Task.WhenAll(phasesTasks))
                .First(response => response != null)!
                .First().Value;

            var userSvt = data.UserSvt
                .GroupBy(svt => svt.Id)
                .ToDictionary(svt => svt.Key, svt => svt.First());

            var spotIconPrefix = spot.WarId.ToString().PadLeft(4, '0');
            var iconUrl = string.Format(
                "https://assets.atlasacademy.io/GameData/NA/Terminal/QuestMap/Capter{0}/QMap_Cap{1}_Atlas/spot_{2}.png",
                spotIconPrefix,
                spotIconPrefix,
                spot.ID.ToString().PadLeft(6, '0')
            );

            var pages = data.EnemyDeck
                .Select((stage, index) =>
                {
                    var embed = new LocalEmbed
                    {
                        Author = new LocalEmbedAuthor
                        {
                            Name = $"{war.Name} > {spot.Name}",
                            IconUrl = iconUrl,
                            Url = $"https://apps.atlasacademy.io/db/NA/war/{war.ID}"
                        },
                        Title = $"`{quest.ID}`. {quest.Name}",
                        Footer = new LocalEmbedFooter().WithText($"AP : {quest.ActConsume}")
                    };

                    var fields = stage.Svts
                        .OrderBy(svt => userSvt[svt.UserSvtId].Id)
                        .Select(svt =>
                        {
                            var usersvt = userSvt[svt.UserSvtId];
                            var traits = usersvt.Individuality.Select(trait => TraitService.GetTrait(trait));
                            return new LocalEmbedField
                            {
                                Name = $"{svt.Id}. {svt.Name}",
                                Value = string.Join(
                                    '\n',
                                    $"HP ; **{usersvt.Hp}** • ATK : **{usersvt.Atk}**",
                                    $"Death rate : {usersvt.DeathRate / 10}% • Critical rate : {usersvt.CriticalRate}%",
                                    $"Charge count : {usersvt.ChargeTurn}",
                                    $"Trait : {string.Join(", ", traits)}"
                                )
                            };
                        });

                    return (
                        new LocalSelectionComponentOption($"Wave {index + 1}", (index + 1).ToString()),
                        new Page().WithEmbeds(embed.WithFields(fields))
                    );
                });

            return Menu(new DefaultMenu(new SelectionPagedView(pages)));
        }
    }
}