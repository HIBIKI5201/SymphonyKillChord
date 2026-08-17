namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     同じ状態効果を再付与した際の処理方法。
    /// </summary>
    public enum StatusEffectReapplyPolicy
    {
        /// <summary> 別効果として重複させる。 </summary>
        Stack = 0,
        /// <summary> 効果時間を延長する。 </summary>
        ExtendDuration = 1,
        /// <summary> 効果時間を初期値に戻す。 </summary>
        RefreshDuration = 2,
        /// <summary> 新しい効果へ上書き。</summary>
        Replace = 3,
        /// <summary> 無視する。 </summary>
        Ignore = 4
    }
}
