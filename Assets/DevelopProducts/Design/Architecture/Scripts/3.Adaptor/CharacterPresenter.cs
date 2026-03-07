using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Adaptor
{
    /// <summary>
    ///     ドメインモデルとビューの仲介を行うプレゼンタークラス。
    /// </summary>
    public class CharacterPresenter
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="entity"> キャラクターエンティティ。 </param>
        /// <param name="view"> ビューインターフェース。 </param>
        public CharacterPresenter(CharacterEntity entity, ICharacterView view)
        {
            _entity = entity;
            _view = view;

            _entity.OnHealthChanged += HandleHealthChanged;
            _view.UpdateHealth(_entity.CurrentHealth, _entity.MaxHealth);
        }

        /// <summary> 管理しているキャラクターエンティティ。 </summary>
        public CharacterEntity Entity => _entity;

        /// <summary>
        ///     キャラクターを移動させる。
        /// </summary>
        /// <param name="dir"> 移動方向。 </param>
        public void Move(Vector2 dir)
        {
            _view.Move(dir * _entity.Speed);
        }

        /// <summary>
        ///     体力の変化をハンドルする。
        /// </summary>
        /// <param name="currentHealth"> 現在の体力。 </param>
        private void HandleHealthChanged(float currentHealth)
        {
            _view.UpdateHealth(currentHealth, _entity.MaxHealth);
        }

        private readonly CharacterEntity _entity;
        private readonly ICharacterView _view;
    }
}
