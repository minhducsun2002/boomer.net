using Pepper.Structures.External.FGO.MasterData;

namespace Pepper.Structures.External.FGO.Renderer
{
    public static class EnemyActionFilter
    {
        public static bool IsPlayerAction(MstFunc function)
        {
            if ((TargetTypeText.ApplyTarget) function.ApplyTarget == TargetTypeText.ApplyTarget.PlayerAndEnemy)
            {
                return true;
            }

            bool targetEnemies = false, targetPlayers = false;
            switch ((TargetTypeText.TargetType) function.TargetType)
            {
                case TargetTypeText.TargetType.Enemy:
                case TargetTypeText.TargetType.EnemyAnother:
                case TargetTypeText.TargetType.EnemyAll:
                case TargetTypeText.TargetType.EnemyFull:
                case TargetTypeText.TargetType.EnemyOther:
                case TargetTypeText.TargetType.EnemyRandom:
                case TargetTypeText.TargetType.EnemyOtherFull:
                case TargetTypeText.TargetType.EnemyOneAnotherRandom:
                    targetEnemies = (TargetTypeText.ApplyTarget) function.ApplyTarget ==
                                    TargetTypeText.ApplyTarget.Enemy;
                    break;
                default:
                    targetPlayers = (TargetTypeText.ApplyTarget) function.ApplyTarget ==
                                    TargetTypeText.ApplyTarget.Player;
                    break;
            }

            return targetPlayers || targetEnemies;
        }
    }
}