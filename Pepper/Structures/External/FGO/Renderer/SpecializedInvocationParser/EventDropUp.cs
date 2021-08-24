using System;
using System.Collections.Generic;
using FgoExportedConstants;
using Humanizer;
using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public static partial class SpecializedInvocationParser
    {
        public static (string, Dictionary<string, string[]>, string) EventDropUp(MstFunc function, MstEvent @event, MstItem item, Dictionary<string, string[]> stats)
        {
            if (function.Type != (int) FuncList.TYPE.EVENT_DROP_UP)
                throw new ArgumentException(
                    $"{nameof(function)} must be of type EventDropUp. Received type {function.Type}");

            int increase = default;
            var extraStats = new Dictionary<string, string[]>();
            if (stats.TryGetValue("AddCount", out var addCounts))
                if (addCounts!.Length == 1)
                    increase = int.Parse(addCounts[0]);
                else
                    extraStats["Drop amount increased"] = addCounts;

            var ended = @event.EndedAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var diff = TimeSpan.FromSeconds(Math.Abs(@event.EndedAt - DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

            return (
                    $"**Increase drop amount** of [{item.Name}]" + (increase != default ? $" by {increase}" : ""),
                    extraStats,
                    $"Only during event **{@event.Name}** ({(ended ? "ended" : $"ending in {diff.Humanize()}")})" 
                );
        }
    }
}