using UnityEngine;

namespace DevelopProducts.Architecture.Adaptor
{
    /// <summary>
    ///     キャラクターのビューを抽象化するインターフェース。
    /// </summary>
    public interface ICharacterViewModel
    {
        /// <summary>
        ///     キャラクターを移動させる。
        /// </summary>
        /// <param name="dir"> 移動方向。 </param>
        public void Move(Vector2 dir);

        /// <summary>
        ///     体力の表示を更新する。
        /// </summary>
        /// <param name="currentHealth"> 現在の体力。 </param>
        /// <param name="maxHealth"> 最大体力。 </param>
        public void UpdateHealth(float currentHealth, float maxHealth);
    }
}
