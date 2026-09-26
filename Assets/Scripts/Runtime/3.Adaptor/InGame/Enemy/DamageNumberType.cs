using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     ダメージ数値の種類を表す列挙型。
    ///     値はプレハブの表示スタイルにシリアライズされるため、既存の値を変えない（2 は廃止したスキルダメージ）。
    /// </summary>
    public enum DamageNumberType
    {
        /// <summary> 通常ダメージ </summary>
        Normal = 0,
        /// <summary> クリティカルダメージ（背景の絵の設定に使う） </summary>
        Critical = 1,
        /// <summary> ジャストヒットダメージ </summary>
        JustHit = 3
    }
}
