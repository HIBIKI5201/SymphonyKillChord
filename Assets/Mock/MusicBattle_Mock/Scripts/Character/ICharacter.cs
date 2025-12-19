using UnityEngine;

namespace Mock.MusicBattle.Character
{
    /// <summary>
    ///     キャラクターの共通インターフェース。
    /// </summary>
    public interface ICharacter
    {
        /// <summary> このキャラクターに関連付けられたGameObjectを取得します。 </summary>
        public GameObject gameObject { get; }
        /// <summary> このキャラクターのピボット位置を取得します。 </summary>
        public Vector3 Pivot { get; }

        /// <summary>
        ///     このキャラクターにダメージを与えます。
        /// </summary>
        /// <param name="damage">与えるダメージ量。</param>
        public void TakeDamage(float damage);
    }
}
