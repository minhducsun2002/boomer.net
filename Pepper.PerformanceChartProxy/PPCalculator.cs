using System.Collections.Generic;
using System.Linq;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Pepper.Commons.Osu;

namespace Pepper.PerformanceChartProxy
{
    public static class PPCalculator
    {
        private const double AccStart = 93, AccEnd = 100, AccStep = 0.5;
        public static string GenerateChartUrl(this WorkingBeatmap beatmap)
        {
            var difficulty = beatmap.CalculateDifficulty(beatmap.GetDefaultRuleset().RulesetInfo.OnlineID);
            List<(double, double)> ppByAccuracy = new(), ppByCombo = new();
            var maxCombo = difficulty.MaxCombo;

            for (var accuracy = AccStart; accuracy <= AccEnd; accuracy += AccStep)
            {
                var fcScore = new ScoreInfo { MaxCombo = difficulty.MaxCombo, Accuracy = accuracy / 100 };
                fcScore.SetCountMiss(0);
                var pp = beatmap.GetPerformanceCalculator(fcScore).Calculate();
                ppByAccuracy.Add((accuracy, pp));
            }

            for (var comboFraction = 1; comboFraction <= ppByAccuracy.Count; comboFraction++)
            {
                var combo = (int) (maxCombo * comboFraction / (double) ppByAccuracy.Count);
                var score = new ScoreInfo { MaxCombo = combo, Accuracy = 1.0 };
                var pp = beatmap.GetPerformanceCalculator(score).Calculate();
                ppByCombo.Add((combo, pp));
            }

            var beatmapset = beatmap.Beatmap.Metadata;
            var config = $@"{{
                    type: 'line',
                    data: {{
                        labels: [{string.Join(", ", ppByAccuracy.Select((pair, idx) => $"\"{pair.Item1}%\""))}],
                        datasets: [
                            {{
                                label: 'PP by accuracy (left, assuming FC)',
                                data: [{string.Join(", ", ppByAccuracy.Select(pair => $"{pair.Item2:F2}"))}],
                                borderColor: '#42adf5',
                                backgroundColor: 'purple',
                                xAxisID: 'accuracyX',
                            }},
                            {{
                                label: 'PP by combo (right, assuming highest accuracy)',
                                data: [{string.Join(", ", ppByCombo.Select(pair => $"{pair.Item2:F2}"))}],
                                borderColor: '#ff2a00',
                                backgroundColor: 'black',
                                xAxisID: 'comboX',
                                yAxisID: 'comboY'
                            }},
                        ],
                    }},
                    options: {{
                        title: {{ display: true, text: '{beatmapset.Artist} - {beatmapset.Title}' }},
                        scales : {{
                            comboY: {{ min: {ppByCombo.Min(pair => pair.Item2)}, position: 'right' }},
                            accuracyY: {{ min: {ppByAccuracy.Min(pair => pair.Item2)} }},
                            comboX: {{ 
                                ticks: {{ min: 1, max: {ppByAccuracy.Count}, callback: (v) => Math.floor({maxCombo} * (v + 1) / {ppByAccuracy.Count}) + 'x' }},
                                position: 'top'
                            }},
                            accuracyX: {{ 
                                ticks: {{ min: 1, max: {ppByAccuracy.Count}, callback: (v) => 93 + v / 2 + '%' }},
                                position: 'bottom'
                            }},
                        }}
                    }}    
                }}";

            config = string.Join('\n', config.Split('\n').Select(line => line.Trim()));
            var chart = new QuickChart.Chart
            {
                Height = 600,
                Width = 1000,
                Config = config,
                BackgroundColor = "white"
            };

            return chart.GetUrl() + "&version=3&format=png";
        }
    }
}