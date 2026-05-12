using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッション進行を保持するEntityクラス。
    /// </summary>
    public class MissionProgress
    {
        /// <summary>
        ///     MissionProgress クラスの新しいインスタンスを初期化します。
        /// </summary>
        public MissionProgress()
        {
            _elapsedTime = new MissionElapsedTime(0f);
            _enemyKillRecord = new EnemyKillRecord();
            _endReason = MissionEndReason.None;
        }

        /// <summary> 経過時間を取得します。 </summary>
        public MissionElapsedTime ElapsedTime => _elapsedTime;
        /// <summary> 敵撃破記録を取得します。 </summary>
        public EnemyKillRecord EnemyKillRecord => _enemyKillRecord;

        /// <summary> プレイヤーが死亡したかどうかを取得します。 </summary>
        public bool IsPlayerDead => _isPlayerDead;
        /// <summary> ミッションが終了したかどうかを取得します。 </summary>
        public bool IsFinished => _endReason != MissionEndReason.None;
        /// <summary> ミッションの終了理由を取得します。 </summary>
        public MissionEndReason EndReason => _endReason;

        /// <summary>
        ///     経過時間を進めます。
        /// </summary>
        /// <param name="deltaTime">進める時間。</param>
        public void AdvanceTime(float deltaTime)
        {
            _elapsedTime = _elapsedTime.AdvanceTime(deltaTime);
        }

        /// <summary>
        ///     プレイヤー死亡フラグを立てます。
        /// </summary>
        public void MarkPlayerDead()
        {
            _isPlayerDead = true;
        }

        /// <summary>
        ///     ミッションを終了させます。
        /// </summary>
        /// <param name="reason">終了理由。</param>
        public void Finish(MissionEndReason reason)
        {
            if (reason == MissionEndReason.None)
            {
                Debug.LogWarning("Mission cannot be finished with None reason.");
                return;
            }
            _endReason = reason;
        }

        /// <summary> 経過時間。 </summary>
        private MissionElapsedTime _elapsedTime;
        /// <summary> 敵撃破記録。 </summary>
        private readonly EnemyKillRecord _enemyKillRecord;

        /// <summary> プレイヤー死亡フラグ。 </summary>
        private bool _isPlayerDead;
        /// <summary> 終了理由。 </summary>
        private MissionEndReason _endReason;
    }
}
