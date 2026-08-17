namespace KillChord.Runtime.Domain.InGame.Battle
{
    /// <summary>
    ///     バリアを保持するオブジェクトを表すインターフェース。
    /// </summary>
    public interface IBarrierHolder
    {
        /// <summary> 現在のバリア量を取得する。 </summary>
        float CurrentBarrier { get; }

        /// <summary>
        ///     バリアを追加する。
        /// </summary>
        /// <param name="amount"> 追加するバリアの量。 </param>
        void AddBarrier(float amount);

        /// <summary>
        ///     バリアでダメージを吸収する。
        /// </summary>
        /// <param name="damage"> 吸収するダメージ。 </param>
        /// <param name="absorbedDamage"> 吸収されたダメージ。 </param>
        /// <returns> 吸収後の残りのダメージ。 </returns>
        Damage AbsorbBarrier(Damage damage, out Damage absorbedDamage);

        /// <summary>
        ///     バリアをクリアする。
        /// </summary>
        void ClearBarrier();
    }
}
