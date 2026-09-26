using System;

namespace KillChord.Runtime.Utility.Persistent
{
    /// <summary>
    ///     イベント定義：敵を撃破した時。
    ///     通常敵とボスの双方で、死亡通知を受けた時点で発火する。
    /// </summary>
    public readonly struct EOnEnemyDefeated : IEvent
    {
        /// <summary> 撃破された敵のID。 </summary>
        public readonly Guid DefeatedId;

        /// <summary>
        ///     撃破された敵のIDを指定してイベントを生成するコンストラクタ。
        /// </summary>
        /// <param name="defeatedId"> 撃破された敵のID。</param>
        public EOnEnemyDefeated(Guid defeatedId)
        {
            DefeatedId = defeatedId;
        }
    }
}
