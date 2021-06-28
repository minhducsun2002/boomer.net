using System;
using System.Collections.Generic;

namespace Pepper.Structures.External.FGO.Renderer
{
    public partial class InvocationRenderer
    {
        public readonly static Dictionary<FunctionType, string> FunctionNames = new()
        {
            {FunctionType.AbsorbNpturn, ""},
            {FunctionType.AddState, "Apply"},
            {FunctionType.AddStateShort, "Apply (short)"},
            {FunctionType.BreakGaugeDown, ""},
            {FunctionType.BreakGaugeUp, ""},
            {FunctionType.CallServant, "Summon another servant"},
            {FunctionType.CardReset, "Shuffle all cards"},
            {FunctionType.ChangeBg, ""},
            {FunctionType.ChangeBgm, ""},
            {FunctionType.ChangeServant, ""},
            {FunctionType.ClassDropUp, "Increase drop up for class weak against this servant"},
            {FunctionType.Damage, "Deal damage"},
            {FunctionType.DamageNP, "Deal NP damage"},
            {FunctionType.DamageNPAndCheckIndividuality, ""},
            {FunctionType.DamageNPCounter, ""},
            {FunctionType.DamageNPHpratioHigh, ""},
            {FunctionType.DamageNPHpratioLow, "Deal NP damage based on HP level"},
            {FunctionType.DamageNPIndividual, "Deal Special Attack NP damage"},
            {FunctionType.DamageNPIndividualSum, "Deal Special Attack NP damage (bonus per trait)"},
            {FunctionType.DamageNPPierce, "Deal DEF-ignoring NP damage"},
            {FunctionType.DamageNPRare, "Deal Special Attack NP damage (bonus from rarity)"},
            {FunctionType.DamageNPSafe, ""},
            {FunctionType.DamageNPStateIndividual, ""},
            // deal Special Attack for those with a certain state
            {FunctionType.DamageNPStateIndividualFix, "Deal NP damage & Special Attack NP damage"},
            // deal a static value of damage?
            {FunctionType.DamageValue, "Deal damage"},
            {FunctionType.DelayNpturn, "Drain enemy charge"},
            {FunctionType.DisplayBuffstring, ""},
            {FunctionType.DropUp, ""},
            // https://apps.atlasacademy.io/db/#/NA/skill/990264
            {FunctionType.EnemyEncountCopyRateUp, "Increase rate of enemy copies' appearance"},
            // https://apps.atlasacademy.io/db/#/NA/skill/990317
            {FunctionType.EnemyEncountRateUp, "Increase rate of enemies' appearance"},
            {FunctionType.EnemyProbDown, ""},
            {FunctionType.EventDropRateUp, "Increase event drop rate"},
            {FunctionType.EventDropUp, "Increase event drop"},
            {FunctionType.EventPointRateUp, ""},
            {FunctionType.EventPointUp, "Increase event point gained"},
            {FunctionType.ExpUp, "Increase EXP gained"},
            {FunctionType.ExtendBuffcount, ""},
            {FunctionType.ExtendBuffturn, ""},
            {FunctionType.ExtendSkill, "Increase skill cooldown"},
            {FunctionType.FixCommandcard, "Lock card deck"},
            {FunctionType.ForceAllBuffNoact, ""},
            {FunctionType.ForceInstantDeath, "Apply Instant Death"},
            {FunctionType.FriendPointUp, "Increase FP gained"},
            {FunctionType.FriendPointUpDuplicate, "Increase FP gained (can duplicate)"},
            {FunctionType.GainHp, "Gain HP"},
            {FunctionType.GainHpFromTargets, "Drain HP from target"},
            {FunctionType.GainHpPer, "Gain HP based on percentage"},
            {FunctionType.GainNp, "Gain NP"},
            {FunctionType.GainNpBuffIndividualSum, "Gain NP based on certain state count"},
            {FunctionType.GainNpFromTargets, "Gain NP from target"},
            {FunctionType.GainStar, "Gain Critical Stars"},
            // https://apps.atlasacademy.io/db/#/JP/skill/964246
            {FunctionType.GetRewardGift, ""},
            {FunctionType.HastenNpturn, "Gain charge"},
            {FunctionType.InstantDeath, "Inflict Death"},
            {FunctionType.LossHp, "Reduce HP"},
            {FunctionType.LossHpPer, ""},
            {FunctionType.LossHpPerSafe, ""},
            {FunctionType.LossHpSafe, "Reduce HP without killing"},
            {FunctionType.LossNp, "Decrease NP"},
            {FunctionType.LossStar, "Decrease Critical Stars"},
            // https://youtu.be/lrHzvSckdSY?t=87
            {FunctionType.MovePosition, "Move position of Zeus"},
            {FunctionType.MoveToLastSubmember, "Move to last position in backline"},
            {FunctionType.None, "No effect"},
            {FunctionType.OverwriteDeadType, ""},
            {FunctionType.PtShuffle, "Shuffle party"},
            {FunctionType.QPDropUp, "Increase QP drop rate"},
            {FunctionType.QPUp, "Increase QP gained"},
            {FunctionType.QuickChangeBg, "Change field"},
            {FunctionType.ReleaseState, ""},
            {FunctionType.ReplaceMember, "Replace active party member"},
            {FunctionType.Revival, "Revive"},
            {FunctionType.SendSupportFriendPoint, ""},
            {FunctionType.ServantFriendshipUp, "Increase Bond points gained"},
            {FunctionType.ShortenSkill, "Decrease skill cooldown"},
            {FunctionType.SubState, "Remove effects"},
            {FunctionType.TransformServant, "Swap servant"},
            {FunctionType.UserEquipExpUp, "Increase Mystic Code EXP Gain"}
        };
    }
}