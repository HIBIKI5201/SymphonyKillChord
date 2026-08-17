namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     バフ・デバフを含む状態効果の共通インターフェース。
    /// </summary>
    public interface IStatusEffect
    {
        /// <summary> 状態効果のId。 </summary>
        StatusEffectId Id { get; }

        /// <summary> 状態効果の分類。 </summary>
        StatusEffectCategory Category { get; }

        /// <summary> 状態効果の継続時間。 </summary>
        StatusEffectDuration Duration { get; }

        /// <summary> 再付与時の処理方法です。 </summary>
        StatusEffectReapplyPolicy ReapplyPolicy { get; }
    }
}
