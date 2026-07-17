using System;
using System.Collections.Generic;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤーの攻撃種別ごとのアニメーション設定です。
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerAttackAnimationConfig", menuName = "KillChord/View/Player Attack Animation Config")]
    public sealed class PlayerAttackAnimationConfig : ScriptableObject
    {
        /// <summary> 攻撃種別ごとの設定一覧です。 </summary>
        public IReadOnlyList<PlayerAttackAnimationEntry> Entries => _entries;

        [SerializeField, Tooltip("攻撃結果のBeatTypeごとのアニメーション設定です。")]
        private PlayerAttackAnimationEntry[] _entries = Array.Empty<PlayerAttackAnimationEntry>();
    }
}
