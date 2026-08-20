using System.Collections.Generic;
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
            _damageTaken = new MissionDamageTaken(0f);
            _maxCombo = new MissionCombo(0);
            _usedWeaponIds = new HashSet<string>(System.StringComparer.Ordinal);
            _endReason = MissionEndReason.None;
        }

        /// <summary> 経過時間を取得します。 </summary>
        public MissionElapsedTime ElapsedTime => _elapsedTime;
        /// <summary> 敵撃破記録を取得します。 </summary>
        public EnemyKillRecord EnemyKillRecord => _enemyKillRecord;
        /// <summary> 累計被ダメージを取得します。 </summary>
        public MissionDamageTaken DamageTaken => _damageTaken;
        /// <summary> 使用した武器種類数を取得します。 </summary>
        public MissionWeaponVariety WeaponVariety => new(_usedWeaponIds.Count);
        /// <summary> 最大コンボ数を取得します。 </summary>
        public MissionCombo MaxCombo => _maxCombo;

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
        ///     プレイヤーが受けたダメージを記録します。
        /// </summary>
        /// <param name="damage"> 受けたダメージです。 </param>
        public void RecordDamageTaken(float damage)
        {
            _damageTaken = _damageTaken.Add(damage);
        }

        /// <summary>
        ///     使用した武器を記録します。
        /// </summary>
        /// <param name="weaponId"> 武器を識別するIDです。 </param>
        public void RecordWeaponUse(string weaponId)
        {
            if (string.IsNullOrWhiteSpace(weaponId))
            {
                return;
            }

            _usedWeaponIds.Add(weaponId.Trim());
        }

        /// <summary>
        ///     現在コンボ数から最大コンボを更新します。
        /// </summary>
        /// <param name="combo"> 現在コンボ数です。 </param>
        public void RecordCombo(int combo)
        {
            if (combo <= _maxCombo.Value)
            {
                return;
            }

            _maxCombo = new MissionCombo(combo);
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
        private readonly HashSet<string> _usedWeaponIds;
        private MissionDamageTaken _damageTaken;
        private MissionCombo _maxCombo;

        /// <summary> プレイヤー死亡フラグ。 </summary>
        private bool _isPlayerDead;
        /// <summary> 終了理由。 </summary>
        private MissionEndReason _endReason;
    }
}
