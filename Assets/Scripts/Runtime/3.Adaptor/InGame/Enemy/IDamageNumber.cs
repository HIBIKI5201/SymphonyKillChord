using UnityEngine;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     ダメージ数値を表示するインターフェース。
    /// </summary>
    public interface IDamageNumber
    {
        /// <summary> 
        ///     ダメージ数値を表示する。
        /// </summary>
        /// <param name="damageNumber">ダメージ数値のDTO。</param>
        public void ShowDamage(in DamageNumberDTO damageNumber);
    }
}
