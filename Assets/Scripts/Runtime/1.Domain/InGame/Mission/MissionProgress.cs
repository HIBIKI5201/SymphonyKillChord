using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.Mission
{
    /// <summary>
    ///     ミッション進行を保持するEntityクラス。
    /// </summary>
    public class MissionProgress
    {
        public MissionProgress()
        {
            _elapsedTime = new MissionElapsedTime(0f);
            _enemyKillRecord = new EnemyKillRecord();
            _endReason = MissionEndReason.None;
        }

        public MissionElapsedTime ElapsedTime => _elapsedTime;
        public EnemyKillRecord EnemyKillRecord => _enemyKillRecord;

        public bool IsPlayerDead => _isPlayerDead;
        public bool IsFinished => _endReason != MissionEndReason.None;
        public MissionEndReason EndReason => _endReason;

        public void AdvanceTime(float deltaTime)
        {
            _elapsedTime.AdvanceTime(deltaTime);
        }

        public void MarkPlayerDead()
        {
            _isPlayerDead = true;
        }

        public void Finish(MissionEndReason reason)
        {
            if (reason == MissionEndReason.None)
            {
                Debug.LogWarning("Mission cannot be finished with None reason.");
                return;
            }
            _endReason = reason;
        }

        private MissionElapsedTime _elapsedTime;
        private readonly EnemyKillRecord _enemyKillRecord;

        private bool _isPlayerDead;
        private MissionEndReason _endReason;
    }
}
