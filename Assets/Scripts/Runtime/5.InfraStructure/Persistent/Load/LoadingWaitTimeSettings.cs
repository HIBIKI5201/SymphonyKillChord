using KillChord.Runtime.Application.Persistent.Load;
using KillChord.Runtime.Utility.Constant;
using System;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.Persistent.Load
{
    /// <summary>
    ///     ロード画面の最低表示時間を、遷移元と遷移先の組み合わせごとに決める設定。
    ///     CinemachineのBlend設定のように、全体の値と組み合わせごとの上書きを持つ。
    /// </summary>
    [CreateAssetMenu(
        fileName = nameof(LoadingWaitTimeSettings),
        menuName = PathConst.CREATE_ASSET_MENU_PATH + "Persistent/" + nameof(LoadingWaitTimeSettings))]
    public sealed class LoadingWaitTimeSettings : ScriptableObject, ILoadingWaitTimeResolver
    {
        /// <summary> 組み合わせに当てはまる設定が無いときの最低表示時間（秒）。 </summary>
        public float DefaultMinimumDisplayTime => _defaultMinimumDisplayTime;

        /// <summary>
        ///     遷移元と遷移先に合うロード画面の最低表示時間を返す。
        ///     遷移元と遷移先の両方が一致する設定、遷移先だけの設定、遷移元だけの設定の順に優先する。
        ///     同じ優先度の設定が複数ある場合は、一覧の上にある設定を使う。
        /// </summary>
        /// <param name="fromSceneName"> 遷移元のシーン名。不明な場合はnull。 </param>
        /// <param name="toSceneName"> 遷移先のシーン名。不明な場合はnull。 </param>
        /// <returns> 最低表示時間（秒）。 </returns>
        public float ResolveMinimumDisplayTime(string fromSceneName, string toSceneName)
        {
            float result = _defaultMinimumDisplayTime;
            if (_overrides == null)
            {
                return result;
            }

            int bestPriority = NO_MATCH_PRIORITY;
            foreach (WaitTimeOverride waitTimeOverride in _overrides)
            {
                int priority = waitTimeOverride.GetMatchPriority(fromSceneName, toSceneName);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    result = waitTimeOverride.MinimumDisplayTime;
                }
            }

            return result;
        }

        /// <summary> どの設定にも当てはまらないことを表す優先度。 </summary>
        private const int NO_MATCH_PRIORITY = 0;

        [SerializeField, Min(0f), Tooltip("組み合わせに当てはまる設定が無いときの、ロード画面の最低表示時間（秒）")]
        private float _defaultMinimumDisplayTime = 2f;

        [SerializeField, Tooltip("遷移元と遷移先の組み合わせごとの最低表示時間。シーン名を空欄にすると、どのシーンにも当てはまる")]
        private WaitTimeOverride[] _overrides = Array.Empty<WaitTimeOverride>();

        /// <summary>
        ///     遷移元と遷移先の組み合わせ1件分の最低表示時間。
        /// </summary>
        [Serializable]
        private struct WaitTimeOverride
        {
            /// <summary> この組み合わせでのロード画面の最低表示時間（秒）。 </summary>
            public float MinimumDisplayTime => Mathf.Max(0f, _minimumDisplayTime);

            /// <summary>
            ///     遷移元と遷移先に当てはまるときの優先度を返す。
            /// </summary>
            /// <param name="fromSceneName"> 遷移元のシーン名。 </param>
            /// <param name="toSceneName"> 遷移先のシーン名。 </param>
            /// <returns> 当てはまらない場合は0。両方を指定した設定ほど大きい。 </returns>
            public int GetMatchPriority(string fromSceneName, string toSceneName)
            {
                bool isAnyFrom = string.IsNullOrWhiteSpace(_fromSceneName);
                bool isAnyTo = string.IsNullOrWhiteSpace(_toSceneName);

                // 両方を空欄にした設定は全体の値と同じ意味なので使わない。
                if (isAnyFrom && isAnyTo)
                {
                    return NO_MATCH_PRIORITY;
                }

                if (!isAnyFrom && !string.Equals(_fromSceneName, fromSceneName, StringComparison.Ordinal))
                {
                    return NO_MATCH_PRIORITY;
                }

                if (!isAnyTo && !string.Equals(_toSceneName, toSceneName, StringComparison.Ordinal))
                {
                    return NO_MATCH_PRIORITY;
                }

                if (!isAnyFrom && !isAnyTo)
                {
                    return BOTH_MATCH_PRIORITY;
                }

                return isAnyFrom ? TO_ONLY_MATCH_PRIORITY : FROM_ONLY_MATCH_PRIORITY;
            }

            private const int FROM_ONLY_MATCH_PRIORITY = 1;
            private const int TO_ONLY_MATCH_PRIORITY = 2;
            private const int BOTH_MATCH_PRIORITY = 3;

            [SerializeField, Tooltip("遷移元のシーン名。空欄はどのシーンにも当てはまる")]
            private string _fromSceneName;

            [SerializeField, Tooltip("遷移先のシーン名。空欄はどのシーンにも当てはまる")]
            private string _toSceneName;

            [SerializeField, Min(0f), Tooltip("この組み合わせでのロード画面の最低表示時間（秒）")]
            private float _minimumDisplayTime;
        }
    }
}
