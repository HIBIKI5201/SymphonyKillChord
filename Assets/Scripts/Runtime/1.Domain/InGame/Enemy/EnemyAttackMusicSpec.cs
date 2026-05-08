namespace KillChord.Runtime.Domain.InGame.Enemy
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

        /// <summary> 初回エンカウンター時の攻撃の音楽同期タイミング </summary>
        public EnemyMusicSpec EncounterTiming { get; }
        /// <summary> 2回目以降の攻撃の音楽同期タイミング </summary>
        public EnemyMusicSpec BattleTiming { get; }
    }
}
