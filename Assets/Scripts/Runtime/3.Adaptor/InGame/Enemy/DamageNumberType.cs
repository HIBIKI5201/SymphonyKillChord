using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     ダメージ数値の種類を表す列挙型。
    /// </summary>
    public enum DamageNumberType
    {
        /// <summary> 通常ダメージ </summary>
        Normal,
        /// <summary> クリティカルダメージ </summary>
        Critical,
        /// <summary> スキルダメージ </summary>
        Skill,
    }
}
