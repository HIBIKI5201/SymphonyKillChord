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
        public WeaponItemView WeaponItem => _weaponItem;

        [SerializeField, Tooltip("攻撃結果のBeatType。")]
        private int _beatType;

        [SerializeField, Tooltip("攻撃中だけ表示する武器モデル。")]
        private WeaponItemView _weaponItem;
    }
}
