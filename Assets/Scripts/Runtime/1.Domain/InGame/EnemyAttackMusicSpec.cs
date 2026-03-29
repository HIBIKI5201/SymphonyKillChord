using UnityEngine;

namespace KillChord.Runtime.Domain
{
    /// <summary>
    ///     敵の攻撃に関する音楽同期のタイミング情報をまとめたクラス。
    /// </summary>
    public class EnemyAttackMusicSpec
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
