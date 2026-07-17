using System;

namespace KillChord.Runtime.Adaptor.InGame.Animation
{
    /// <summary>
    ///     キャラクターアニメーションの瞬間イベントとワンショット要求を扱うSignalインターフェース。
    /// </summary>
    public interface ICharacterAnimationSignal
    {
        /// <summary> 回避アニメーションの再生終了イベントです。 </summary>
        public event Action OnDodgeEnded;

        /// <summary>
        ///     回避アニメーションの再生を要求する。
        /// </summary>
        /// <returns> 再生時間です。 </returns>
        public float RequestDodge();

        /// <summary>
        ///     攻撃アニメーションの再生を要求する。
        /// </summary>
        /// <param name="animationKey"> 置き換えたいアニメーションキー。未指定時は既定の攻撃アニメーション。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(string animationKey = null);

        /// <summary>
        ///     攻撃BeatTypeに対応するアニメーションの再生を要求します。
        /// </summary>
        /// <param name="attackType"> 攻撃結果のBeatTypeです。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(int attackType);

        /// <summary>
        ///     任意キーのワンショットアニメーション再生を要求する。
        /// </summary>
        /// <param name="animationKey"> 再生したいアニメーションキー。 </param>
        /// <param name="duration"> 再生時間です。 </param>
        /// <returns> 要求できた場合はtrue。 </returns>
        public bool TryRequestOneShot(string animationKey, out float duration);
    }
}
