using DevelopProducts.Architecture.Application;
using DevelopProducts.Architecture.Domain;
using UnityEngine;

namespace DevelopProducts.Architecture.Adaptor
{
    /// <summary>
    ///     キャラクターの操作を制御するコントローラークラス。
    /// </summary>
    public class CharacterController
    {
        /// <summary>
        ///     コンストラクタ。
        /// </summary>
        /// <param name="attacker"> 攻撃ロジック。 </param>
        /// <param name="presenter"> プレゼンター。 </param>
        public CharacterController(CharacterAttack attacker, CharacterPresenter presenter)
        {
            _attacker = attacker;
            _presenter = presenter;
        }

        /// <summary> 現在のプレゼンター。 </summary>
        public CharacterPresenter Presenter => _presenter;

        /// <summary>
        ///     対象にダメージを与える。
        /// </summary>
        /// <param name="target"> ダメージを与える対象のプレゼンター。 </param>
        public void AddDamage(CharacterPresenter target)
        {
            _attacker.AddDamage(target.Entity);
        }

        /// <summary>
        ///     キャラクターを移動させる。
        /// </summary>
        /// <param name="dir"> 移動方向。 </param>
        public void Move(Vector2 dir)
        {
            _presenter.Move(dir);
        }

        private readonly CharacterPresenter _presenter;
        private readonly CharacterAttack _attacker;
    }
}
