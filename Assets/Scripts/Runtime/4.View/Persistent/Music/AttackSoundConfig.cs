using System;
using UnityEngine;

namespace KillChord.Runtime.View.Persistent.Music
{
    /// <summary>
    ///     BeatTypeとCueNameの対応情報。
    /// </summary>
    [Serializable]
    public struct AttackSoundConfig
    {
        [Tooltip("攻撃結果のBeatType。")]
        public int BeatType;

        [Tooltip("このBeatTypeで使用するSE Source。")]
        public SoundEffectSource Source;

        [Tooltip("再生するCueName。空の場合はSource側のCueを再生する。")]
        public string CueName;
    }
}
