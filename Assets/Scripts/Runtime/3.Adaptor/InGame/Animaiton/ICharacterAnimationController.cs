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
        ///     Viewが購読するイベント。
        /// </summary>
        event Action<int> OnOneShotRequested;

        /// <summary> 現在のアニメーション状態をDTOとして取得する。 </summary>
        CharacterAnimationDTO GetDTO();

        /// <summary> キャラクターの速度ベクトルを設定する。 </summary>
        /// <param name="velocity"> 2D速度ベクトル。 </param>
        void SetVelocity(Vector2 velocity);

        /// <summary> イベント入力が発生したことを通知する。 </summary>
        void TriggerOneShot(int index);

        /// <summary>
        ///    ワンショットアニメーションの再生時間を取得する。
        /// </summary>
        /// <param name="index"> ワンショットアニメーションのインデックス。 </param>
        /// <returns> ワンショットアニメーションの再生時間（秒）。 </returns>
        float GetOneShotAnimationLength(int index);
    }
}
