using JetBrains.Annotations;
using KillChord.Runtime.Domain.OutGame.Screen;
using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

namespace KillChord.Runtime.InfraStructure.OutGame.Screen
{
    /// <summary>
    ///     画面遷移ルールデータ。
    /// </summary>
    [CreateAssetMenu(fileName = "ScreenRuleData", menuName = "KillChord/OutGame/ScreenRuleData")]
    public sealed class ScreenRuleData : ScriptableObject
    {
        /// <summary> 画面遷移ルールエントリの一覧。 </summary>
        [SerializeField, Tooltip("画面ごとの遷移ルール一覧。")]
        private ScreenRuleEntry[] _entries = Array.Empty<ScreenRuleEntry>();

        /// <summary> 画面遷移ルールエントリの一覧を取得します。 </summary>
        public ScreenRuleEntry[] Entries => _entries;

        /// <summary>
        ///     画面遷移ルールエントリ。
        /// </summary>
        [Serializable]
        public struct ScreenRuleEntry
        {
            [SerializeField, Tooltip("対象の画面 ID。")]
            private ScreenId _screenId;

            [SerializeField, Tooltip("遷移タイプ。")]
            private ScreenTransitionType _transitionType;

            [SerializeField, Tooltip("履歴に追加するかどうか。")]
            private bool _isAddToHistory;

            /// <summary> 対象の画面 ID を取得します。 </summary>
            public ScreenId ScreenId => _screenId;
            /// <summary> 遷移タイプを取得します。 </summary>
            public ScreenTransitionType TransitionType => _transitionType;
            /// <summary> 履歴に追加するかどうかを取得します。 </summary>
            public bool IsAddToHistory => _isAddToHistory;
        }
    }
}
