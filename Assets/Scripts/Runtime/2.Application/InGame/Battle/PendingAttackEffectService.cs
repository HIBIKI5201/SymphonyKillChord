using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Application.InGame.Battle
{
    /// <summary>
    ///     次の通常攻撃へ付与する追加効果を管理するサービス。
    /// </summary>
    public class PendingAttackEffectService
    {
        /// <summary>
        ///     次の通常攻撃へ付与する追加効果を登録する。
        /// </summary>
        /// <param name="effect"> 登録する追加効果。 </param>
        public void Register(IAttackHitEffect effect)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }

            _pendingEffects.Add(effect);
        }

        /// <summary>
        ///     登録されている追加効果を消費する。
        /// </summary>
        /// <returns> 消費された追加効果の配列。 </returns>
        public IAttackHitEffect[] Consume()
        {
            if (_pendingEffects.Count == 0)
            {
                return Array.Empty<IAttackHitEffect>();
            }

            var result = _pendingEffects.ToArray();
            _pendingEffects.Clear();

            return result;
        }

        /// <summary>
        ///     登録されている追加効果をすべてクリアする。
        /// </summary>
        public void Clear()
        {
            _pendingEffects.Clear();
        }

        private readonly List<IAttackHitEffect> _pendingEffects = new List<IAttackHitEffect>();
    }
}
