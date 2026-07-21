using KillChord.Runtime.Domain.InGame.Music;

namespace KillChord.Runtime.Domain.InGame.Enemy
{
    /// <summary>
    ///     敵の攻撃に関する音楽同期のタイミング情報をまとめたクラス。
    /// </summary>
    public readonly struct EnemyAttackMusicSpec
    {
        public EnemyAttackMusicSpec(MusicSyncSpec encounterTiming, MusicSyncSpec battleTiming)
        {
            EncounterTiming = encounterTiming;
            BattleTiming = battleTiming;
        }

        /// <summary> 初回エンカウンター時の攻撃の音楽同期タイミング </summary>
        public MusicSyncSpec EncounterTiming { get; }
        /// <summary> 2回目以降の攻撃の音楽同期タイミング </summary>
        public MusicSyncSpec BattleTiming { get; }
    }
}
