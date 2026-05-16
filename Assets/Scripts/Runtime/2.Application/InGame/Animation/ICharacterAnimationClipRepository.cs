using KillChord.Runtime.Domain;
using UnityEngine;

namespace KillChord.Runtime.Application
{
    /// <summary>
    ///     アニメーションクリップを取得するリポジトリインターフェース。
    /// </summary>
    public interface ICharacterAnimationClipRepository
    {
        /// <summary> アニメーション状態からクリップを検索する。 </summary>
        /// <param name="state"> アニメーション状態。 </param>
        /// <param name="clip"> 取得したアニメーションクリップ。 </param>
        /// <returns> 取得成功かどうか。 </returns>
        bool TryFindByState(CharacterAnimationState state, out AnimationClip clip);
    }
}
