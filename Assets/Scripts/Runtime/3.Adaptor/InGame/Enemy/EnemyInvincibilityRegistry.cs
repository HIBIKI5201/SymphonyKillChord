using KillChord.Runtime.Application.InGame.Buff;
using KillChord.Runtime.Domain.InGame.Character;
using KillChord.Runtime.Domain.InGame.StatusEffect;
using System;
using System.Collections.Generic;

namespace KillChord.Runtime.Adaptor.InGame.Enemy
{
    /// <summary>
    ///     出現中の敵を保持し、無敵状態の付与・解除を管理するクラス。
    /// </summary>
    public class EnemyInvincibilityRegistry
    {
        /// <summary>
        ///     空の登録簿を生成する。敵は無敵でない状態で始まる。
        /// </summary>
        public EnemyInvincibilityRegistry()
        {
            _activeEnemies = new();
            _invincibleBuff = new InvincibleBuff();
            _isInvincible = false;
        }

        /// <summary> 敵が無敵状態か否か </summary>
        public bool IsInvincible => _isInvincible;

        /// <summary>
        ///     出現した敵を登録する。無敵状態中なら無敵状態を付与する。
        /// </summary>
        /// <param name="enemyEntity"> 登録する敵のEntity。 </param>
        public void Register(CharacterEntity enemyEntity)
        {
            if (enemyEntity == null)
            {
                throw new ArgumentNullException(nameof(enemyEntity));
            }

            // 再出現時は状態効果がリセットされているため、登録済みでも付与し直す。
            _activeEnemies.Add(enemyEntity);
            if (_isInvincible)
            {
                ApplyInvincible(enemyEntity, true);
            }
        }

        /// <summary>
        ///     敵の登録を解除し、付与中の無敵状態を解除する。
        /// </summary>
        /// <param name="enemyEntity"> 登録を解除する敵のEntity。 </param>
        public void Unregister(CharacterEntity enemyEntity)
        {
            if (enemyEntity == null || !_activeEnemies.Remove(enemyEntity))
            {
                return;
            }

            ApplyInvincible(enemyEntity, false);
        }

        /// <summary>
        ///     登録中の敵と、これから出現する敵の無敵状態を切り替える。
        /// </summary>
        /// <param name="isInvincible"> 無敵にする場合はtrue、解除する場合はfalse。 </param>
        public void SetInvincible(bool isInvincible)
        {
            if (isInvincible == _isInvincible) return;
            _isInvincible = isInvincible;
            foreach (CharacterEntity enemyEntity in _activeEnemies)
            {
                ApplyInvincible(enemyEntity, isInvincible);
            }
        }

        /// <summary>
        ///     敵へ無敵状態を付与または解除する。
        ///     状態効果だけでは確定ダメージが被ダメージの補正のあとに加算されて通るため、
        ///     Entityの無敵フラグも合わせて切り替え、ダメージを受けない状態にする。
        /// </summary>
        /// <param name="enemyEntity"> 対象の敵のEntity。 </param>
        /// <param name="isInvincible"> 無敵にする場合はtrue、解除する場合はfalse。 </param>
        private void ApplyInvincible(CharacterEntity enemyEntity, bool isInvincible)
        {
            enemyEntity.SetInvincible(isInvincible);
            if (isInvincible)
            {
                enemyEntity.StatusEffectSystem.Add(_invincibleBuff);
            }
            else
            {
                enemyEntity.StatusEffectSystem.Remove(_invincibleBuff);
            }
        }

        private readonly HashSet<CharacterEntity> _activeEnemies;
        private readonly IStatusEffect _invincibleBuff;
        private bool _isInvincible;
    }
}
