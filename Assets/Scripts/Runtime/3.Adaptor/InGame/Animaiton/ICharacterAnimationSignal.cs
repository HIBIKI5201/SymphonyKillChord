namespace KillChord.Runtime.Adaptor.InGame.Animation
{
    /// <summary>
    ///     キャラクターアニメーションの瞬間イベントとワンショット要求を扱うSignalインターフェース。
    /// </summary>
    public interface ICharacterAnimationSignal
    {
        /// <summary>
        ///     既定の攻撃アニメーションの再生を要求する。
        /// </summary>
        /// <returns> 再生時間です。 </returns>
        public float RequestAttack();

        /// <summary>
        ///     任意キーのワンショットアニメーション再生を要求する。
        /// </summary>
        /// <param name="animationKey"> 再生したいアニメーションキー。 </param>
        /// <param name="duration"> 再生時間です。 </param>
        /// <returns> 要求できた場合はtrue。 </returns>
        public bool TryRequestOneShot(string animationKey, out float duration);
    }
}
