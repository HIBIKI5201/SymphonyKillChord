using System;

namespace KillChord.Runtime.Adaptor.InGame.Animation
{
    /// <summary>
    ///     プレイヤー固有のキャラクターアニメーション要求を扱うSignalインターフェース。
    /// </summary>
    public interface IPlayerCharacterAnimationSignal : ICharacterAnimationSignal
    {
        /// <summary> 回避アニメーションの再生終了イベントです。 </summary>
        public event Action OnDodgeEnded;

        /// <summary>
        ///     回避アニメーションの再生を要求する。
        /// </summary>
        /// <returns> 再生時間です。 </returns>
        public float RequestDodge();

        /// <summary>
        ///     指定キーの攻撃アニメーションの再生を要求する。
        /// </summary>
        /// <param name="animationKey"> 置き換えたいアニメーションキー。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(string animationKey);

        /// <summary>
        ///     攻撃BeatTypeに対応するアニメーションの再生を要求する。
        /// </summary>
        /// <param name="attackType"> 攻撃結果のBeatTypeです。 </param>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack(int attackType);
    }
}
