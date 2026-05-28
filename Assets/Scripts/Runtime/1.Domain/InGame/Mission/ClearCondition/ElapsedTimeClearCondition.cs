using System;

namespace KillChord.Runtime.Domain.InGame.Mission.ClearCondition
{
    /// <summary>
    ///     一定時間経過でクリアとなる条件。
    ///     生存ミッションなどで使用されることを想定しています。
    /// </summary>
    public class ElapsedTimeClearCondition : IMissionClearCondition
    {
        /// <summary>
        ///     ElapsedTimeClearCondition クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="requiredTime">必要な経過時間。</param>
        public ElapsedTimeClearCondition(float requiredTime)
        {
            if (requiredTime < 0f || float.IsNaN(requiredTime) || float.IsInfinity(requiredTime))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requiredTime),
                    "requiredTime must be non-negative and finite.");
            }

            _requiredTime = requiredTime;
        }

        /// <summary>
        ///     条件の説明文を取得します。
        /// </summary>
        /// <returns>説明文。</returns>
        public string GetDescription()
        {
            return $"{_requiredTime}秒生存する。";
        }

        /// <summary>
        ///     条件が満たされているかどうかを判定します。
        /// </summary>
        /// <param name="progress">ミッションの進行状況。</param>
        /// <returns>条件を満たしている場合は true、そうでない場合は false。</returns>
        public bool IsSatisfied(MissionProgress progress)
        {
            return progress.ElapsedTime.Value >= _requiredTime;
        }

        /// <summary> 必要な経過時間。 </summary>
        private readonly float _requiredTime;
    }
}
