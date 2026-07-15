using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Enemy
{
    /// <summary>
    ///     足元の判定結果と足音SEの対応情報。
    /// </summary>
    [Serializable]
    public struct FootstepSoundConfig
    {
        [Tooltip("足元判定で使用するレイヤー。")]
        public LayerMask SurfaceLayer;

        [Tooltip("この床で再生するCueName。空の場合は共通Cueを使用します。")]
        public string CueName;
    }
}
