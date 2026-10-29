namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     効果時間が切れて削除された際に処理を行う状態効果。
    /// </summary>
    public interface IStatusEffectExpiredHandler
    {
        /// <summary>
        ///     効果時間が切れて削除された際の処理を行う。
        /// </summary>
        void OnExpired();
    }
}
