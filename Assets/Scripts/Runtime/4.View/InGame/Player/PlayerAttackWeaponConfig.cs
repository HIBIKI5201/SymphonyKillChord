using KillChord.Runtime.View.Persistent.Music;
using System;
using UnityEngine;

namespace KillChord.Runtime.View.InGame.Player
{
    /// <summary>
    ///     BeatTypeごとの攻撃武器やSEの設定を保持する構造体。
    /// </summary>
    [Serializable]
    public struct PlayerAttackWeaponConfig
    {
        /// <summary>
        ///     攻撃結果のBeatType。
        /// </summary>
        public int BeatType => _beatType;

        /// <summary>
        ///     攻撃中だけ表示する武器モデル。
        /// </summary>
        public GameObject WeaponModel => _weaponModel;

        /// <summary>
        ///     攻撃SE用Source。
        /// </summary>
        public SoundEffectSource AttackSoundSource => _attackSoundSource;

        /// <summary>
        ///     再生するCueName。
        /// </summary>
        public string CueName => _cueName;

        [SerializeField, Tooltip("攻撃結果のBeatType。")]
        private int _beatType;

        [SerializeField, Tooltip("攻撃中だけ表示する武器モデル。")]
        private GameObject _weaponModel;

        [SerializeField, Tooltip("攻撃SE用Source。")]
        private SoundEffectSource _attackSoundSource;

        [SerializeField, Tooltip("再生するCueName。空の場合はSource側のCueを再生します。")]
        private string _cueName;
    }
}
