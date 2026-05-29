namespace DevelopProducts.BehaviorGraph.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃に関する音楽同期のタイミング情報をまとめたクラス。
    /// </summary>
    public readonly struct EnemyAttackMusicSpec
    {
        public EnemyAttackMusicSpec(EnemyMusicSpec encounterTiming, EnemyMusicSpec battleTiming)
        {
            EncounterTiming = encounterTiming;
            BattleTiming = battleTiming;
        }

        public EnemyMusicSpec EncounterTiming { get; }
        public EnemyMusicSpec BattleTiming { get; }
    }
}
