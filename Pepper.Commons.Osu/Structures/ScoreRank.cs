namespace Pepper.Commons.Osu.API
{
    public enum ScoreRank
    {
        F = -1,
        D = osu.Game.Scoring.ScoreRank.D,
        C = osu.Game.Scoring.ScoreRank.C,
        B = osu.Game.Scoring.ScoreRank.B,
        A = osu.Game.Scoring.ScoreRank.A,
        S = osu.Game.Scoring.ScoreRank.S,
        SH = osu.Game.Scoring.ScoreRank.SH,
        X = osu.Game.Scoring.ScoreRank.X,
        XH = osu.Game.Scoring.ScoreRank.XH,
        // Ripple support
        SS = X,
        SSH = XH,
    }
}