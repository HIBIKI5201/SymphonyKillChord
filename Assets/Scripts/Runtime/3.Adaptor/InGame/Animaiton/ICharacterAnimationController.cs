using System;
using UnityEngine;

namespace KillChord.Runtime.Adaptor
{
    /// <summary>
    ///     キャラクターアニメーション操作のAdaptorインターフェース。
    /// </summary>
    public interface ICharacterAnimationController
    {
        /// <summary>
        ///     Viewが購読する攻撃要求イベント。
        /// </summary>
        event Action OnAttackRequested;

        /// <summary> 現在のアニメーション状態をDTOとして取得する。 </summary>
        CharacterAnimationDTO GetDTO();

        /// <summary> キャラクターの速度ベクトルを設定する。 </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        void SetVelocity(Vector2 velocity);
        
        /// <summary> 攻撃入力が発生したことを通知する。 </summary>
        void TriggerAttack();
    }
}
