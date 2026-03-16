using UnityEngine;

namespace DevelopProducts.Design.GameMode.Domain
{
    /// <summary>
    ///     敵のランタイム状態を管理するクラス。
    ///     例えば、敵の残り体力、など、敵に関連するランタイムの情報を管理するためのクラス。
    /// </summary>
    public class EnemyRuntimeState
    {
        public EnemyRuntimeState(int maxHp)
        {
            _maxHp = maxHp;
            _currentHp = maxHp;
        }

        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public bool IsDead => _isDead;

        /// <summary>
        ///     ダメージを受けるメソッド。
        ///     HPが0以下になった場合、プレイヤーは死亡状態になる。
        /// </summary>
        /// <param name="damage"></param>
        public void TakeDamage(int damage)
        {
            if (_isDead) return;
            _currentHp -= damage;

            if (_currentHp <= 0)
            {
                _currentHp = 0;
                _isDead = true;
            }
        }

        private int _currentHp;
        private int _maxHp;
        private bool _isDead;
    }
}
