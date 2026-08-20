using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     指定した敵を一定数以上倒すとクリアとなる条件。
    /// </summary>
    public class EnemyKillCountClearCondition : IMissionClearCondition
    {
        /// <summary>
        ///     EnemyKillCountClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="key">敵のキー。</param>
        /// <param name="requiredKillCount">必要な撃破数。</param>
        /// <param name="displayName">表示名。</param>
        public EnemyKillCountClearCondition(EnemyMissionKey key, int requiredKillCount, string displayName)
        {
            if (requiredKillCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredKillCount),
                    requiredKillCount,
                    "必要撃破数は1以上である必要があります。");
            }

            _key = key;
            _requiredKillCount = requiredKillCount;
            _displayName = displayName;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return $"{_displayName}を{_requiredKillCount}体以上倒す。";
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.EnemyKillRecord.GetKillCount(_key) >= _requiredKillCount;
        }

        /// <summary> 敵のキー。 </summary>
        private readonly EnemyMissionKey _key;
        /// <summary> 必要な撃破数。 </summary>
        private readonly int _requiredKillCount;
        /// <summary> 表示名。 </summary>
        private readonly string _displayName;
    }
}
