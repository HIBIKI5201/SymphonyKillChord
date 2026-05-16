using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     キャラクターアニメーション操作のAdaptorインターフェース。
    /// </summary>
    public interface ICharacterAnimationController
    {
        /// <summary> 現在のアニメーション状態をDTOとして取得する。 </summary>
        CharacterAnimationDTO GetDTO();

        /// <summary> キャラクターの速度ベクトルを設定する。 </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        void SetVelocity(Vector2 velocity);
    }
}
