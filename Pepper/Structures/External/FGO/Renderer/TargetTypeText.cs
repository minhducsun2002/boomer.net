namespace Pepper.Structures.External.FGO.Renderer
{
    internal static class TargetTypeText
    {
        public enum ApplyTarget
        {
            Player = 1,
            Enemy,
            PlayerAndEnemy
        }
        
        public enum TargetType
        {
            Self = 0,
            PtOne = 1,
            PtAnother = 2,
            PtAll = 3,
            Enemy = 4,
            EnemyAnother = 5,
            EnemyAll = 6,
            PtFull = 7,
            EnemyFull = 8,
            PtOther = 9,
            PtOneOther = 10,
            PtRandom = 11,
            EnemyOther = 12,
            EnemyRandom = 13,
            PtOtherFull = 14,
            EnemyOtherFull = 15,
            PtselectOneSub = 16,
            PtselectSub = 17,
            PtOneAnotherRandom = 18,
            PtSelfAnotherRandom = 19,
            EnemyOneAnotherRandom = 20,
            PtSelfAnotherFirst = 21,
            PtSelfBefore = 22,
            PtSelfAfter = 23,
            PtSelfAnotherLast = 24,
            CommandTypeSelfTreasureDevice = 25,
            FieldOther = 26,
            EnemyOneNoTargetNoAction = 27,
            PtOneHpLowestValue = 28,
            PtOneHpLowestRate = 29
        }
        
        public static string ResolveText(int type)
        {
            var casted = (TargetType) type;
            return casted switch
            {
                TargetType.Self => "self",
                TargetType.PtOne => "a chosen ally",
                TargetType.PtAll => "all allies on field",
                TargetType.Enemy => "an enemy on field",
                TargetType.EnemyAll => "all enemies on field",
                TargetType.PtFull => "all allies",
                TargetType.EnemyFull => "all enemies",
                TargetType.PtOther => "other allies on field",
                TargetType.PtOneOther => "other allies on field except a chosen one",
                TargetType.PtRandom => "a random ally on field",
                TargetType.EnemyRandom => "a random enemy on field",
                TargetType.PtOtherFull => "other allies",
                TargetType.PtselectOneSub => "a chosen reserve ally",
                TargetType.PtSelfAnotherRandom => "a random ally except self",
                TargetType.PtSelfAnotherFirst => "first ally except self",
                TargetType.PtSelfAnotherLast => "last ally except self",
                TargetType.FieldOther => "everyone on field except self",
                TargetType.EnemyOneNoTargetNoAction => "one last dealt damage to self",
                TargetType.PtOneHpLowestValue => "ally with lowest HP",
                TargetType.PtOneHpLowestRate => "ally with lowest HP percentage",
                _ => $"[Target {type}]"
            };
        }
    }
}