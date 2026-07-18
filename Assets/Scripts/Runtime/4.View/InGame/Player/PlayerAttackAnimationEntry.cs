using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     プレイヤーの攻撃種別とアニメーションクリップの対応設定です。
    /// </summary>
    [Serializable]
    public struct PlayerAttackAnimationEntry
    {
        /// <summary> 攻撃結果のBeatTypeです。 </summary>
        public int BeatType => _beatType;

        /// <summary> 再生するアニメーションクリップです。 </summary>
        public AnimationClip Clip => _clip;

        /// <summary> このクリップ専用の開始ブレンドフレーム数です。 </summary>
        public int EnterBlendFrameCount => _enterBlendFrameCount;

        /// <summary> このクリップ専用の終了ブレンドフレーム数です。 </summary>
        public int ExitBlendFrameCount => _exitBlendFrameCount;

        [SerializeField, Min(1), Tooltip("このアニメーションを再生する攻撃結果のBeatTypeです。")]
        private int _beatType;

        [SerializeField, Tooltip("この攻撃で再生するアニメーションクリップです。")]
        private AnimationClip _clip;

        [SerializeField, Min(0), Tooltip("このクリップ専用の開始ブレンドフレーム数です。0の場合は共通Configの既定値を使用します。")]
        private int _enterBlendFrameCount;

        [SerializeField, Min(0), Tooltip("このクリップ専用の終了ブレンドフレーム数です。0の場合は共通Configの既定値を使用します。")]
        private int _exitBlendFrameCount;
    }
}
