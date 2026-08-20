using KillChord.Runtime.Domain.InGame.Battle;
using UnityEngine;

namespace KillChord.Runtime.Domain.InGame.StatusEffect
{
    /// <summary>
    ///     ダメージを受けた際に発動する効果を表すインターフェース。
    /// </summary>
    public interface IDamageTakenHandler
    {
        /// <summary>
        ///     ダメージを受けた際に発動する処理。
        /// </summary>
        /// <param name="context"> ダメージを受けた際の文脈情報。 </param>
        void OnDamageTaken(in DamageTakenContext context);
    }
}
